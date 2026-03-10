using ESCL8.Domain.Enums;

namespace ESCL8.Domain.Entities;

public class Ambulance
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Hospital/provider that owns the ambulance
    public Guid CompanyId { get; set; }

    // If true, this ambulance is part of the public EMS pool
    public bool IsPublic { get; set; }

    public string DisplayName { get; set; } = "Ambulance";

    public AmbulanceStatus Status { get; set; } = AmbulanceStatus.Offline;

    // Last known position (used for closest-distance auto dispatch)
    public double? LastLatitude { get; set; }
    public double? LastLongitude { get; set; }
    public DateTime? LastSeenUtc { get; set; }
}