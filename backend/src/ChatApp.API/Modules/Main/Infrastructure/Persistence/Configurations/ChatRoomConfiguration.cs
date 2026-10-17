using ChatApp.API.Modules.Main.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.API.Modules.Main.Infrastructure.Persistence.Configurations;

public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.ToTable("ChatRooms");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.CreatedByUserId).IsRequired();
        builder.Property(x => x.IsGroup).IsRequired();

        builder.HasMany(x => x.Members)
               .WithOne(m => m.ChatRoom)
               .HasForeignKey(m => m.ChatRoomId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Members)
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.LastMessageAt);
        builder.HasIndex(x => x.CreatedByUserId);
    }
}