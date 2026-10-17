using ChatApp.API.Modules.Main.Domain;
using ChatApp.API.Modules.Shared.Infrastructure.Persistence.DbContexts;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Modules.Main.Infrastructure.Persistence;

public class MainDbContext : BaseDbContext
{
    public MainDbContext(DbContextOptions<MainDbContext> options, IPublisher publisher)
        : base(options, publisher)
    {
    }

    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<ChatRoomMember> ChatRoomMembers => Set<ChatRoomMember>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MainDbContext).Assembly);
    }
}