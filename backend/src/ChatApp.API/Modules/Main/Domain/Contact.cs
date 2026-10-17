using ChatApp.API.Modules.Shared.Kernel.Entities;
using ChatApp.API.Modules.Shared.Kernel.Exceptions;

namespace ChatApp.API.Modules.Main.Domain;

public class Contact : AggregateRoot
{
    public Guid OwnerUserId { get; private set; }
    public Guid ContactUserId { get; private set; }
    public string? Alias { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }

    private Contact() { }

    public static Contact Create(Guid ownerUserId, Guid contactUserId, string? alias = null)
    {
        if (ownerUserId == contactUserId)
            throw new DomainException("You cannot add yourself as a contact.");

        return new Contact
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            ContactUserId = contactUserId,
            Alias = alias?.Trim(),
            AddedAt = DateTimeOffset.UtcNow
        };
    }

    public void Rename(string? alias) => Alias = alias?.Trim();
}