using ESCL8.Application.Interfaces;
using ESCL8.Domain.Enums;
using ESCL8.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ESCL8.Infrastructure.Services;

public class AutoAssignRetryHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutoAssignRetryHostedService> _log;

    // tweakables (MVP)
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan MinAgeBeforeRetry = TimeSpan.FromSeconds(15);

    public AutoAssignRetryHostedService(IServiceScopeFactory scopeFactory, ILogger<AutoAssignRetryHostedService> log)
    {
        _scopeFactory = scopeFactory;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("AutoAssignRetryHostedService started. Interval={Interval}s", Interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<Escl8DbContext>();
                var auto = scope.ServiceProvider.GetRequiredService<IAutoDispatchService>();

                var cutoff = DateTime.UtcNow - MinAgeBeforeRetry;

                var candidates = await db.Incidents
                    .Where(i => i.Status == IncidentStatus.Open
                             && i.CreatedUtc <= cutoff
                             && i.AssignedAmbulanceId == null
                             && i.Latitude != null
                             && i.Longitude != null)
                    .OrderBy(i => i.CreatedUtc)
                    .Take(25)
                    .ToListAsync(stoppingToken);

                foreach (var incident in candidates)
                {
                    var ok = await auto.TryAutoAssignAsync(incident.Id);
                    _log.LogInformation("AutoRetry incident {IncidentId} assigned={Assigned}", incident.Id, ok);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "AutoAssignRetryHostedService loop error");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}