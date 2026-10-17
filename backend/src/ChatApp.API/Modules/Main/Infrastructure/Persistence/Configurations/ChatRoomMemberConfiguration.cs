using ChatApp.API.Modules.Main.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.API.Modules.Main.Infrastructure.Persistence.Configurations;

public class ChatRoomMemberConfiguration : IEntityTypeConfiguration<ChatRoomMember>
{
    public void Configure(EntityTypeBuilder<ChatRoomMember> builder)
    {
        builder.ToTable("ChatRoomMembers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.JoinedAt).IsRequired();

        builder.HasIndex(x => new { x.ChatRoomId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}