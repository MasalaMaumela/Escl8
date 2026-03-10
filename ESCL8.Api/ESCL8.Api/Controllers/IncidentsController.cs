using ESCL8.Application.DTOs;
using ESCL8.Application.Interfaces;
using ESCL8.Domain.Entities;
using ESCL8.Domain.Enums;
using ESCL8.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESCL8.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly Escl8DbContext _db;
    private readonly IAutoDispatchService _auto;
    private readonly ILogger<IncidentsController> _log;

    public IncidentsController(Escl8DbContext db, IAutoDispatchService auto, ILogger<IncidentsController> log)
    {
        _db = db;
        _auto = auto;
        _log = log;
    }

    // ---------------------------
    // CREATE (public allowed)
    // ---------------------------
    [HttpPost]
    public async Task<ActionResult<Incident>> Create([FromBody] CreateIncidentRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Description)) return BadRequest("Description is required.");
        if (req.Latitude is null || req.Longitude is null) return BadRequest("Latitude and Longitude are required.");
        if (req.Latitude < -90 || req.Latitude > 90) return BadRequest("Latitude out of range.");
        if (req.Longitude < -180 || req.Longitude > 180) return BadRequest("Longitude out of range.");

        // Private incidents must have company
        if (!req.IsPublicIncident && req.CompanyId == Guid.Empty)
            return BadRequest("CompanyId is required for non-public incidents.");

        var incident = new Incident
        {
            CompanyId = req.IsPublicIncident ? Guid.Empty : req.CompanyId,
            IsPublicIncident = req.IsPublicIncident,
            Description = req.Description.Trim(),
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            Status = IncidentStatus.Open,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };

        _db.Incidents.Add(incident);
        await _db.SaveChangesAsync();

        // Auto-assign if possible (non-fatal)
        var autoAssigned = await _auto.TryAutoAssignAsync(incident.Id);
        _log.LogInformation("Incident {IncidentId} created. AutoAssigned={AutoAssigned}", incident.Id, autoAssigned);

        // Return created (client can GET /details)
        return CreatedAtAction(nameof(GetById), new { id = incident.Id }, incident);
    }

    // ---------------------------
    // BASIC GETS
    // ---------------------------
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Incident>> GetById(Guid id)
    {
        var incident = await _db.Incidents.FirstOrDefaultAsync(x => x.Id == id);
        if (incident is null) return NotFound();
        return Ok(incident);
    }

    // GET api/v1/incidents?companyId=...
    [HttpGet]
    public async Task<ActionResult<List<Incident>>> List([FromQuery] Guid? companyId)
    {
        var q = _db.Incidents.AsQueryable();

        if (companyId.HasValue && companyId.Value != Guid.Empty)
            q = q.Where(x => x.CompanyId == companyId.Value);

        var items = await q.OrderByDescending(x => x.CreatedUtc).Take(200).ToListAsync();
        return Ok(items);
    }

    // ✅ FRONTEND-FRIENDLY: active incidents
    // GET api/v1/incidents/active?companyId=...&publicOnly=true|false
    [HttpGet("active")]
    public async Task<ActionResult<List<Incident>>> Active([FromQuery] Guid? companyId, [FromQuery] bool? publicOnly)
    {
        var q = _db.Incidents.AsQueryable();

        q = q.Where(i => i.Status == IncidentStatus.Open
                      || i.Status == IncidentStatus.Assigned
                      || i.Status == IncidentStatus.EnRoute);

        if (publicOnly.HasValue)
            q = q.Where(i => i.IsPublicIncident == publicOnly.Value);

        if (companyId.HasValue && companyId.Value != Guid.Empty)
            q = q.Where(i => i.CompanyId == companyId.Value);

        var items = await q.OrderByDescending(i => i.CreatedUtc).Take(200).ToListAsync();
        return Ok(items);
    }

    // ✅ FRONTEND-FRIENDLY: details
    // GET api/v1/incidents/{id}/details
    [HttpGet("{id:guid}/details")]
    public async Task<ActionResult<IncidentDetailsResponse>> Details(Guid id)
    {
        var incident = await _db.Incidents.FirstOrDefaultAsync(x => x.Id == id);
        if (incident is null) return NotFound();

        Ambulance? amb = null;
        if (incident.AssignedAmbulanceId.HasValue)
            amb = await _db.Ambulances.FirstOrDefaultAsync(a => a.Id == incident.AssignedAmbulanceId.Value);

        var latest = await _db.LocationPings
            .Where(p => p.IncidentId == id)
            .OrderByDescending(p => p.TimestampUtc)
            .FirstOrDefaultAsync();

        return Ok(new IncidentDetailsResponse
        {
            Incident = incident,
            AssignedAmbulance = amb,
            LatestLocation = latest
        });
    }

    // ---------------------------
    // DISPATCH ACTIONS (dispatcher-protected by middleware in Phase 5)
    // ---------------------------

    // POST api/v1/incidents/{id}/assign
    [HttpPost("{id:guid}/assign")]
    public async Task<ActionResult<Incident>> Assign(Guid id, [FromBody] AssignAmbulanceRequest req)
    {
        if (req.AmbulanceId == Guid.Empty) return BadRequest("AmbulanceId is required.");

        var incident = await _db.Incidents.FirstOrDefaultAsync(x => x.Id == id);
        if (incident is null) return NotFound();

        if (incident.Status != IncidentStatus.Open)
            return BadRequest("Incident must be Open to assign.");

        var amb = await _db.Ambulances.FirstOrDefaultAsync(a => a.Id == req.AmbulanceId);
        if (amb is null) return BadRequest("Ambulance not found.");

        if (amb.Status != AmbulanceStatus.Standby)
            return BadRequest("Ambulance must be Standby to assign.");

        // match rules
        if (incident.IsPublicIncident && !amb.IsPublic)
            return BadRequest("Public incident requires a public ambulance.");
        if (!incident.IsPublicIncident && (amb.IsPublic || amb.CompanyId != incident.CompanyId))
            return BadRequest("Private incident requires ambulance from same company.");

        incident.AssignedAmbulanceId = amb.Id;
        incident.Status = IncidentStatus.Assigned;
        incident.AssignedUtc = DateTime.UtcNow;
        incident.UpdatedUtc = DateTime.UtcNow;

        amb.Status = AmbulanceStatus.Busy;
        amb.LastSeenUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(incident);
    }

    // POST api/v1/incidents/{id}/reassign
    [HttpPost("{id:guid}/reassign")]
    public async Task<ActionResult<Incident>> Reassign(Guid id, [FromBody] ReassignAmbulanceRequest req)
    {
        if (req.NewAmbulanceId == Guid.Empty) return BadRequest("NewAmbulanceId is required.");

        var incident = await _db.Incidents.FirstOrDefaultAsync(x => x.Id == id);
        if (incident is null) return NotFound();

        // do not reassign completed/cancelled
        if (incident.Status is IncidentStatus.Resolved or IncidentStatus.Cancelled)
            return BadRequest("Cannot reassign a closed incident.");

        var newAmb = await _db.Ambulances.FirstOrDefaultAsync(a => a.Id == req.NewAmbulanceId);
        if (newAmb is null) return BadRequest("New ambulance not found.");

        if (newAmb.Status != AmbulanceStatus.Standby)
            return BadRequest("New ambulance must be Standby.");

        // match rules
        if (incident.IsPublicIncident && !newAmb.IsPublic)
            return BadRequest("Public incident requires a public ambulance.");
        if (!incident.IsPublicIncident && (newAmb.IsPublic || newAmb.CompanyId != incident.CompanyId))
            return BadRequest("Private incident requires ambulance from same company.");

        // return old ambulance to standby if we can
        if (incident.AssignedAmbulanceId.HasValue)
        {
            var oldAmb = await _db.Ambulances.FirstOrDefaultAsync(a => a.Id == incident.AssignedAmbulanceId.Value);
            if (oldAmb is not null)
            {
                // If already enroute/arrived, dispatcher should use policy; MVP: still allow, but return to standby
                oldAmb.Status = AmbulanceStatus.Standby;
                oldAmb.LastSeenUtc = DateTime.UtcNow;
            }
        }

        incident.AssignedAmbulanceId = newAmb.Id;
        incident.Status = IncidentStatus.Assigned;
        incident.AssignedUtc ??= DateTime.UtcNow; // keep if already assigned before
        incident.UpdatedUtc = DateTime.UtcNow;

        newAmb.Status = AmbulanceStatus.Busy;
        newAmb.LastSeenUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(incident);
    }

    // POST api/v1/incidents/{id}/unassign
    [HttpPost("{id:guid}/unassign")]
    public async Task<ActionResult<Incident>> Unassign(Guid id)
    {
        var incident = await _db.Incidents.FirstOrDefaultAsync(x => x.Id == id);
        if (incident is null) return NotFound();

        if (incident.AssignedAmbulanceId is null)
            return BadRequest("Incident is not assigned.");

        if (incident.Status is IncidentStatus.Resolved or IncidentStatus.Cancelled)
            return BadRequest("Cannot unassign a closed incident.");

        var amb = await _db.Ambulances.FirstOrDefaultAsync(a => a.Id == incident.AssignedAmbulanceId.Value);
        if (amb is not null)
        {
            amb.Status = AmbulanceStatus.Standby;
            amb.LastSeenUtc = DateTime.UtcNow;
        }

        incident.AssignedAmbulanceId = null;

        // reset lifecycle (MVP policy)
        incident.AssignedUtc = null;
        incident.EnRouteUtc = null;
        incident.ArrivedUtc = null;
        incident.ResolvedUtc = null;
        incident.CancelledUtc = null;

        incident.Status = IncidentStatus.Open;
        incident.UpdatedUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(incident);
    }

    // POST api/v1/incidents/{id}/retry-auto-assign
    [HttpPost("{id:guid}/retry-auto-assign")]
    public async Task<ActionResult> RetryAutoAssign(Guid id)
    {
        var incident = await _db.Incidents.FirstOrDefaultAsync(x => x.Id == id);
        if (incident is null) return NotFound();

        if (incident.Status != IncidentStatus.Open)
            return BadRequest("Incident must be Open.");

        var success = await _auto.TryAutoAssignAsync(id);
        var updated = await _db.Incidents.FirstAsync(x => x.Id == id);

        return Ok(new { success, incident = updated });
    }

    // ---------------------------
    // STATUS UPDATES (dispatcher or ambulance app - protected by middleware in Phase 5)
    // ---------------------------
    [HttpPost("{id:guid}/status")]
    public async Task<ActionResult<Incident>> UpdateStatus(Guid id, [FromBody] UpdateIncidentStatusRequest req)
    {
        var incident = await _db.Incidents.FirstOrDefaultAsync(x => x.Id == id);
        if (incident is null) return NotFound();

        // rules: only allow valid transitions
        if (!IsValidTransition(incident.Status, req.Status))
            return BadRequest($"Invalid status transition: {incident.Status} -> {req.Status}");

        incident.Status = req.Status;
        incident.UpdatedUtc = DateTime.UtcNow;

        var now = DateTime.UtcNow;
        switch (req.Status)
        {
            case IncidentStatus.EnRoute:
                if (incident.AssignedAmbulanceId is null) return BadRequest("Cannot set EnRoute without an assigned ambulance.");
                incident.EnRouteUtc = now;
                break;
            case IncidentStatus.Arrived:
                incident.ArrivedUtc = now;
                break;
            case IncidentStatus.Resolved:
                incident.ResolvedUtc = now;
                break;
            case IncidentStatus.Cancelled:
                incident.CancelledUtc = now;

                // return ambulance to standby if assigned
                if (incident.AssignedAmbulanceId.HasValue)
                {
                    var amb = await _db.Ambulances.FirstOrDefaultAsync(a => a.Id == incident.AssignedAmbulanceId.Value);
                    if (amb is not null)
                    {
                        amb.Status = AmbulanceStatus.Standby;
                        amb.LastSeenUtc = DateTime.UtcNow;
                    }
                }
                break;
        }

        await _db.SaveChangesAsync();
        return Ok(incident);
    }

    private static bool IsValidTransition(IncidentStatus from, IncidentStatus to)
    {
        // Allowed:
        // Open -> Assigned -> EnRoute -> Arrived -> Resolved
        // Open/Assigned/EnRoute -> Cancelled
        return (from, to) switch
        {
            (IncidentStatus.Open, IncidentStatus.Assigned) => true,
            (IncidentStatus.Assigned, IncidentStatus.EnRoute) => true,
            (IncidentStatus.EnRoute, IncidentStatus.Arrived) => true,
            (IncidentStatus.Arrived, IncidentStatus.Resolved) => true,

            (IncidentStatus.Open, IncidentStatus.Cancelled) => true,
            (IncidentStatus.Assigned, IncidentStatus.Cancelled) => true,
            (IncidentStatus.EnRoute, IncidentStatus.Cancelled) => true,

            // allow idempotent updates (same status) without error
            _ when from == to => true,

            _ => false
        };
    }

    // ---------------------------
    // LOCATION PINGS
    // ---------------------------

    // POST api/v1/incidents/{id}/location
    [HttpPost("{id:guid}/location")]
    public async Task<ActionResult<LocationPing>> CreateLocationPing(Guid id, [FromBody] CreateLocationPingRequest req)
    {
        if (req.AmbulanceId == Guid.Empty) return BadRequest("AmbulanceId is required.");
        if (req.Latitude < -90 || req.Latitude > 90) return BadRequest("Latitude out of range.");
        if (req.Longitude < -180 || req.Longitude > 180) return BadRequest("Longitude out of range.");

        var incidentExists = await _db.Incidents.AnyAsync(x => x.Id == id);
        if (!incidentExists) return NotFound("Incident not found.");

        // Optional: ensure ping is from the assigned ambulance
        var incident = await _db.Incidents.FirstAsync(x => x.Id == id);
        if (incident.AssignedAmbulanceId.HasValue && incident.AssignedAmbulanceId.Value != req.AmbulanceId)
            return BadRequest("This ambulance is not assigned to the incident.");

        var ping = new LocationPing
        {
            IncidentId = id,
            AmbulanceId = req.AmbulanceId,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            TimestampUtc = DateTime.UtcNow
        };

        _db.LocationPings.Add(ping);
        await _db.SaveChangesAsync();

        return Ok(ping);
    }

    // GET api/v1/incidents/{id}/location/latest
    [HttpGet("{id:guid}/location/latest")]
    public async Task<ActionResult<LocationPing>> GetLatestLocationPing(Guid id)
    {
        var ping = await _db.LocationPings
            .Where(x => x.IncidentId == id)
            .OrderByDescending(x => x.TimestampUtc)
            .FirstOrDefaultAsync();

        if (ping is null) return NotFound("No location pings yet.");

        return Ok(ping);
    }

    // GET api/v1/incidents/{id}/location/history?minutes=10
    [HttpGet("{id:guid}/location/history")]
    public async Task<ActionResult<List<LocationPing>>> GetLocationHistory(Guid id, [FromQuery] int minutes = 10)
    {
        if (minutes < 1) minutes = 1;
        if (minutes > 120) minutes = 120;

        var since = DateTime.UtcNow.AddMinutes(-minutes);

        var pings = await _db.LocationPings
            .Where(x => x.IncidentId == id && x.TimestampUtc >= since)
            .OrderBy(x => x.TimestampUtc)
            .ToListAsync();

        return Ok(pings);
    }
}