using ESCL8.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ESCL8.Infrastructure.Persistence;

public class Escl8DbContext : DbContext
{
    public Escl8DbContext(DbContextOptions<Escl8DbContext> options)
        : base(options) { }

    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<LocationPing> LocationPings => Set<LocationPing>();
    public DbSet<Ambulance> Ambulances => Set<Ambulance>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // 🔹 Incident audit timestamps
        foreach (var entry in ChangeTracker.Entries<Incident>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedUtc = now;
                entry.Entity.UpdatedUtc = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedUtc = now;
            }
        }

        // 🔹 Ambulance heartbeat / activity tracking
        foreach (var entry in ChangeTracker.Entries<Ambulance>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.LastSeenUtc = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}