using ChatApp.API.Modules.Main.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.API.Modules.Main.Infrastructure.Persistence.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Alias).HasMaxLength(100);

        builder.HasIndex(x => new { x.OwnerUserId, x.ContactUserId }).IsUnique();
        builder.HasIndex(x => x.OwnerUserId);
    }
}