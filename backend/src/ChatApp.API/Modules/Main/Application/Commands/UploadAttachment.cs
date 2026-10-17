using ChatApp.API.Modules.Main.Domain;
using ChatApp.API.Modules.Main.Infrastructure.Persistence;
using ChatApp.API.Modules.Shared.Infrastructure.Storage;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using FluentValidation;

namespace ChatApp.API.Modules.Main.Application.Commands;

public static class UploadAttachment
{
    public record Command(
        Stream Stream,
        string FileName,
        string ContentType,
        long Size) : IRequest<Result<Guid>>;

    public class Validation : AbstractValidator<Command>
    {
        public Validation()
        {
            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required.")
                .MaximumLength(255).WithMessage("File name must not exceed 255 characters.");

            RuleFor(x => x.Size)
                .GreaterThan(0).WithMessage("File is empty.")
                .LessThanOrEqualTo(10 * 1024 * 1024).WithMessage("File must not exceed 10 MB.");
        }
    }

    public class Handler : IRequestHandler<Command, Result<Guid>>
    {
        private readonly MainDbContext _db;
        private readonly IFileStorage _fileStorage;
        private readonly ICurrentUser _currentUser;

        public Handler(MainDbContext db, IFileStorage fileStorage, ICurrentUser currentUser)
        {
            _db = db;
            _fileStorage = fileStorage;
            _currentUser = currentUser;
        }

        public async Task<Result<Guid>> Handle(Command request, CancellationToken ct)
        {
            if (_currentUser.UserId is null)
                return Result.Failure<Guid>(
                    new Error("Attachment.Unauthenticated", "Not authenticated.", ErrorType.Unauthorized));

            var userId = _currentUser.UserId.Value;
            var objectName = $"{Guid.NewGuid():N}{Path.GetExtension(request.FileName)}";

            await _fileStorage.UploadAsync(
                request.Stream, objectName, request.ContentType, request.Size, ct);

            var attachment = Attachment.Create(
                userId, objectName, request.FileName, request.ContentType, request.Size);

            _db.Attachments.Add(attachment);
            await _db.SaveChangesAsync(ct);

            return Result.Success(attachment.Id);
        }
    }
}