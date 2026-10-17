using ChatApp.API.Modules.Main.Domain;
using ChatApp.API.Modules.Main.Infrastructure.Persistence;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Modules.Main.Application.Commands;

public static class AddContact
{
    public record Command(Guid ContactUserId, string? Alias) : IRequest<Result<Guid>>;

    public class Validation : AbstractValidator<Command>
    {
        public Validation()
        {
            RuleFor(x => x.ContactUserId)
                .NotEmpty().WithMessage("Contact user id is required.");

            RuleFor(x => x.Alias)
                .MaximumLength(100).WithMessage("Alias must not exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Alias));
        }
    }

    public class Handler : IRequestHandler<Command, Result<Guid>>
    {
        private readonly MainDbContext _db;
        private readonly ICurrentUser _currentUser;

        public Handler(MainDbContext db, ICurrentUser currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<Result<Guid>> Handle(Command request, CancellationToken ct)
        {
            if (_currentUser.UserId is null)
                return Result.Failure<Guid>(
                    new Error("Contact.Unauthenticated", "Not authenticated.", ErrorType.Unauthorized));

            var ownerId = _currentUser.UserId.Value;

            var exists = await _db.Contacts
                .AnyAsync(c => c.OwnerUserId == ownerId && c.ContactUserId == request.ContactUserId, ct);

            if (exists)
                return Result.Failure<Guid>(
                    new Error("Contact.Duplicate", "Contact already exists.", ErrorType.Conflict));

            try
            {
                var contact = Contact.Create(ownerId, request.ContactUserId, request.Alias);
                _db.Contacts.Add(contact);
                await _db.SaveChangesAsync(ct);
                return Result.Success(contact.Id);
            }
            catch (Shared.Kernel.Exceptions.DomainException ex)
            {
                return Result.Failure<Guid>(
                    new Error("Contact.Invalid", ex.Message, ErrorType.Validation));
            }
        }
    }
}