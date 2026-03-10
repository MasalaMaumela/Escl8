using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ESCL8.Domain.Enums;

namespace ESCL8.Application.DTOs;

public class UpdateAmbulanceStatusRequest
{
    public AmbulanceStatus Status { get; set; }
}
