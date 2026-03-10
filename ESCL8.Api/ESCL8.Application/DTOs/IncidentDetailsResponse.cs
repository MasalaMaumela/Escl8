using ESCL8.Domain.Entities;

namespace ESCL8.Application.DTOs;

public class IncidentDetailsResponse
{
    public Incident Incident { get; set; } = default!;
    public Ambulance? AssignedAmbulance { get; set; }
    public LocationPing? LatestLocation { get; set; }

    public object Timeline => new
    {
        Incident.CreatedUtc,
        Incident.AssignedUtc,
        Incident.EnRouteUtc,
        Incident.ArrivedUtc,
        Incident.ResolvedUtc,
        Incident.CancelledUtc
    };
}


