using ChatApp.API.Modules.Main.Application.DTOs;
using ChatApp.API.Modules.Main.Infrastructure.Persistence;
using ChatApp.API.Modules.Shared.Infrastructure.Storage;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Modules.Main.Application.Queries;

public static class GetAttachmentFile
{
    public record Query(Guid AttachmentId) : IRequest<Result<AttachmentFileDto>>;

    public class Handler : IRequestHandler<Query, Result<AttachmentFileDto>>
    {
        private readonly MainDbContext _db;
        private readonly IFileStorage _storage;
        private readonly ICurrentUser _currentUser;

        public Handler(MainDbContext db, IFileStorage storage, ICurrentUser currentUser)
        {
            _db = db;
            _storage = storage;
            _currentUser = currentUser;
        }

        public async Task<Result<AttachmentFileDto>> Handle(Query request, CancellationToken ct)
        {
            if (_currentUser.UserId is null)
                return Result.Failure<AttachmentFileDto>(
                    new Error("File.Unauthenticated", "Not authenticated.", ErrorType.Unauthorized));

            var attachment = await _db.Attachments
                .FirstOrDefaultAsync(a => a.Id == request.AttachmentId, ct);

            if (attachment is null)
                return Result.Failure<AttachmentFileDto>(
                    new Error("File.NotFound", "Attachment not found.", ErrorType.NotFound));

            var stream = await _storage.DownloadAsync(attachment.FileId, ct);

            return Result.Success(new AttachmentFileDto(
                stream, attachment.OriginalFileName, attachment.ContentType));
        }
    }
}