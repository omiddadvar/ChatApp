using ChatApp.API.Modules.Shared.Infrastructure.Persistence.DbContexts;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Modules.Notification.Infrastructure.Persistence;

public class NotificationDbContext : BaseDbContext
{
    public NotificationDbContext(
        DbContextOptions<NotificationDbContext> options,
        IPublisher publisher) : base(options, publisher) { }

    public DbSet<Domain.Notification> Notifications => Set<Domain.Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}