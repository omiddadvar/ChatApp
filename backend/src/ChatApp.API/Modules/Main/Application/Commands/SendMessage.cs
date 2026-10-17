using ChatApp.API.Modules.Main.Application.Interfaces;
using ChatApp.API.Modules.Main.Domain.Documents;
using ChatApp.API.Modules.Main.Domain.Enums;
using ChatApp.API.Modules.Main.Infrastructure.Persistence;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Modules.Main.Application.Commands;

public static class SendMessage
{
    public record Command(
        Guid ChatRoomId,
        string Content,
        Guid? AttachmentId = null,
        MessageType MessageType = MessageType.Text) : IRequest<Result<Guid>>;

    public class Validation : AbstractValidator<Command>
    {
        public Validation()
        {
            RuleFor(x => x.ChatRoomId)
                .NotEmpty().WithMessage("Chat room id is required.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Message content is required.")
                .MaximumLength(4000).WithMessage("Message must not exceed 4000 characters.");

            RuleFor(x => x.MessageType)
                .IsInEnum().WithMessage("Invalid message type.")
                .Must(t => t != MessageType.Unknown)
                .WithMessage("Message type must be specified.");

            RuleFor(x => x.AttachmentId)
                .NotNull()
                .WithMessage("Attachment id is required for image and file messages.")
                .When(x => x.MessageType is MessageType.Image or MessageType.File);
        }
    }

    public class Handler : IRequestHandler<Command, Result<Guid>>
    {
        private readonly MainDbContext _db;
        private readonly IChatHistoryRepository _history;
        private readonly ICurrentUser _currentUser;

        public Handler(
            MainDbContext db,
            IChatHistoryRepository history,
            ICurrentUser currentUser)
        {
            _db = db;
            _history = history;
            _currentUser = currentUser;
        }

        public async Task<Result<Guid>> Handle(Command request, CancellationToken ct)
        {
            if (_currentUser.UserId is null)
                return Result.Failure<Guid>(
                    new Error("Message.Unauthenticated", "Not authenticated.", ErrorType.Unauthorized));

            var userId = _currentUser.UserId.Value;

            var room = await _db.ChatRooms
                .Include(r => r.Members)
                .FirstOrDefaultAsync(r => r.Id == request.ChatRoomId, ct);

            if (room is null)
                return Result.Failure<Guid>(
                    new Error("Message.RoomNotFound", "Chat room not found.", ErrorType.NotFound));

            if (!room.HasMember(userId))
                return Result.Failure<Guid>(
                    new Error("Message.NotMember", "You are not a member of this room.", ErrorType.Forbidden));

            if (request.AttachmentId is not null)
            {
                var attachmentExists = await _db.Attachments
                    .AnyAsync(a => a.Id == request.AttachmentId, ct);

                if (!attachmentExists)
                    return Result.Failure<Guid>(
                        new Error("Message.AttachmentNotFound", "Attachment not found.", ErrorType.NotFound));
            }

            var messageId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            var document = new ChatMessageDocument
            {
                Id = messageId,
                ChatRoomId = room.Id,
                SenderUserId = userId,
                Content = request.Content.Trim(),
                AttachmentId = request.AttachmentId,
                SentAt = now,
                MessageType = request.MessageType
            };

            await _history.InsertAsync(document, ct);

            room.MarkMessageActivity(now);
            await _db.SaveChangesAsync(ct);

            return Result.Success(messageId);
        }
    }
}