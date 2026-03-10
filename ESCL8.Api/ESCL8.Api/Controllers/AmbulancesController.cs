using ESCL8.Application.DTOs;
using ESCL8.Domain.Entities;
using ESCL8.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESCL8.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AmbulancesController : ControllerBase
{
    private readonly Escl8DbContext _db;

    public AmbulancesController(Escl8DbContext db)
    {
        _db = db;
    }

    // POST api/v1/ambulances
    // Fast onboarding: register an ambulance under a company (hospital/provider) or public pool
    [HttpPost]
    public async Task<ActionResult<Ambulance>> Register([FromBody] RegisterAmbulanceRequest req)
    {
        if (req.CompanyId == Guid.Empty && !req.IsPublic)
            return BadRequest("CompanyId is required for non-public ambulances.");

        var amb = new Ambulance
        {
            CompanyId = req.IsPublic ? Guid.Empty : req.CompanyId,
            IsPublic = req.IsPublic,
            DisplayName = string.IsNullOrWhiteSpace(req.DisplayName) ? "Ambulance" : req.DisplayName.Trim()
        };

        _db.Ambulances.Add(amb);
        await _db.SaveChangesAsync();

        return Ok(amb);
    }

    // GET api/v1/ambulances?companyId=...&isPublic=true|false
    [HttpGet]
    public async Task<ActionResult<List<Ambulance>>> List([FromQuery] Guid? companyId, [FromQuery] bool? isPublic)
    {
        var q = _db.Ambulances.AsQueryable();

        if (companyId.HasValue && companyId.Value != Guid.Empty)
            q = q.Where(x => x.CompanyId == companyId.Value);

        if (isPublic.HasValue)
            q = q.Where(x => x.IsPublic == isPublic.Value);

        var items = await q.OrderByDescending(x => x.LastSeenUtc).Take(200).ToListAsync();
        return Ok(items);
    }

    // GET api/v1/ambulances/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Ambulance>> GetById(Guid id)
    {
        var amb = await _db.Ambulances.FirstOrDefaultAsync(x => x.Id == id);
        if (amb is null) return NotFound();
        return Ok(amb);
    }

    // POST api/v1/ambulances/{id}/status
    [HttpPost("{id:guid}/status")]
    public async Task<ActionResult<Ambulance>> UpdateStatus(Guid id, [FromBody] UpdateAmbulanceStatusRequest req)
    {
        var amb = await _db.Ambulances.FirstOrDefaultAsync(x => x.Id == id);
        if (amb is null) return NotFound();

        amb.Status = req.Status;
        amb.LastSeenUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(amb);
    }

    // POST api/v1/ambulances/{id}/location
    [HttpPost("{id:guid}/location")]
    public async Task<ActionResult<Ambulance>> UpdateLocation(Guid id, [FromBody] UpdateAmbulanceLocationRequest req)
    {
        var amb = await _db.Ambulances.FirstOrDefaultAsync(x => x.Id == id);
        if (amb is null) return NotFound();

        amb.LastLatitude = req.Latitude;
        amb.LastLongitude = req.Longitude;
        amb.LastSeenUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(amb);
    }
}