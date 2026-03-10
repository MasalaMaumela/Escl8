namespace ESCL8.Domain.Entities;

public class LocationPing
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid IncidentId { get; set; }

    // ✅ Track the ambulance unit (not a person)
    public Guid AmbulanceId { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}