using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESCL8.Application.DTOs;

public class RegisterAmbulanceRequest
{
    public Guid CompanyId { get; set; }
    public bool IsPublic { get; set; }
    public string DisplayName { get; set; } = "Ambulance";
}
