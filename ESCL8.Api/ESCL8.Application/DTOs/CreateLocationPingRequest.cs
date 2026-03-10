namespace ESCL8.Application.DTOs;

public class CreateLocationPingRequest
{
    public Guid AmbulanceId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}