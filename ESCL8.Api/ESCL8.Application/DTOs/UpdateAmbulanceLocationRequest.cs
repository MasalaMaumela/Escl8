using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ESCL8.Application.DTOs;

public class UpdateAmbulanceLocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
