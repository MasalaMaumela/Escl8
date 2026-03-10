using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESCL8.Application.DTOs;

public class CreateIncidentRequest
{
    public Guid CompanyId { get; set; }
    public string Description { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsPublicIncident { get; set; } = false;

}