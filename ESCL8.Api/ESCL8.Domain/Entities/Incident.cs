using ESCL8.Domain.Enums;

namespace ESCL8.Domain.Entities;

public class Incident
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Tenant/hospital/provider
    public Guid CompanyId { get; set; }

    // Public EMS pool incident
    public bool IsPublicIncident { get; set; } = false;

    public string Description { get; set; } = string.Empty;

    public IncidentStatus Status { get; set; } = IncidentStatus.Open;

    // Optional link to requester (future users/auth)
    public Guid? RequestedByUserId { get; set; }

    // Location
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Audit
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    // ✅ Split: Ambulance vs Responder(User)
    public Guid? AssignedAmbulanceId { get; set; }        // used now
    public Guid? AssignedResponderUserId { get; set; }    // future optional

    // Lifecycle timestamps
    public DateTime? AssignedUtc { get; set; }
    public DateTime? EnRouteUtc { get; set; }
    public DateTime? ArrivedUtc { get; set; }
    public DateTime? ResolvedUtc { get; set; }
    public DateTime? CancelledUtc { get; set; }
}