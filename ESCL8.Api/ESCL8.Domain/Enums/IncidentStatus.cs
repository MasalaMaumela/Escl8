using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESCL8.Domain.Enums;

public enum IncidentStatus
{
    Open = 1,
    Assigned = 2,
    EnRoute = 3,
    Arrived = 4,
    Resolved = 5,
    Cancelled = 6
}