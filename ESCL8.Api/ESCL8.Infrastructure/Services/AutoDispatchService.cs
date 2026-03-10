using ESCL8.Application.Interfaces;
using ESCL8.Domain.Enums;
using ESCL8.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ESCL8.Infrastructure.Services;

public class AutoDispatchService : IAutoDispatchService
{
    private readonly Escl8DbContext _db;

    public AutoDispatchService(Escl8DbContext db)
    {
        _db = db;
    }

    public async Task<bool> TryAutoAssignAsync(Guid incidentId)
    {
        var incident = await _db.Incidents.FirstOrDefaultAsync(x => x.Id == incidentId);
        if (incident is null) return false;

        if (incident.Latitude is null || incident.Longitude is null) return false;
        if (incident.Status != IncidentStatus.Open) return false;

        var q = _db.Ambulances.Where(a =>
            a.Status == AmbulanceStatus.Standby &&
            a.LastLatitude != null &&
            a.LastLongitude != null
        );

        // public vs private matching
        if (incident.IsPublicIncident)
            q = q.Where(a => a.IsPublic);
        else
            q = q.Where(a => !a.IsPublic && a.CompanyId == incident.CompanyId);

        var candidates = await q.ToListAsync();
        if (candidates.Count == 0) return false;

        var best = candidates
            .Select(a => new
            {
                Ambulance = a,
                DistanceKm = HaversineKm(
                    incident.Latitude.Value, incident.Longitude.Value,
                    a.LastLatitude!.Value, a.LastLongitude!.Value
                )
            })
            .OrderBy(x => x.DistanceKm)
            .ThenByDescending(x => x.Ambulance.LastSeenUtc) // tie-breaker
            .First();

        // ✅ Assign ambulance
        incident.AssignedAmbulanceId = best.Ambulance.Id;
        incident.Status = IncidentStatus.Assigned;
        incident.AssignedUtc = DateTime.UtcNow;
        incident.UpdatedUtc = DateTime.UtcNow;

        best.Ambulance.Status = AmbulanceStatus.Busy;
        best.Ambulance.LastSeenUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRad(double x) => x * (Math.PI / 180.0);
}