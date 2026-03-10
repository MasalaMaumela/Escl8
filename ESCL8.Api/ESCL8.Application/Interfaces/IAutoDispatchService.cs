using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ESCL8.Application.Interfaces;

public interface IAutoDispatchService
{
    Task<bool> TryAutoAssignAsync(Guid incidentId);
}