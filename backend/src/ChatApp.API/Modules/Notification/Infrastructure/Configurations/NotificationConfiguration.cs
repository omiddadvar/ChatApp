using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.API.Modules.Notification.Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Domain.Notification>
{
    public void Configure(EntityTypeBuilder<Domain.Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(2000);
        builder.Property(x => x.PayloadJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Type).HasConversion<byte>();

        builder.HasIndex(x => new { x.UserId, x.ReadAt });
        builder.HasIndex(x => new { x.UserId, x.DeliveredAt });
        builder.HasIndex(x => x.CreatedAt);
    }
}