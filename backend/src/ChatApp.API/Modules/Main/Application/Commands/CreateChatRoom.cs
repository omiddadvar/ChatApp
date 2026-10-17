using ChatApp.API.Modules.Main.Domain;
using ChatApp.API.Modules.Main.Infrastructure.Persistence;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using FluentValidation;

namespace ChatApp.API.Modules.Main.Application.Commands;

public static class CreateChatRoom
{
    public record Command(
        bool IsGroup,
        string? Name,
        Guid? OtherUserId,
        IReadOnlyList<Guid>? MemberIds) : IRequest<Result<Guid>>;

    public class Validation : AbstractValidator<Command>
    {
        public Validation()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Group name is required for group chats.")
                .MaximumLength(100).WithMessage("Group name must not exceed 100 characters.")
                .When(x => x.IsGroup);

            RuleFor(x => x.OtherUserId)
                .NotNull().WithMessage("Other user id is required for direct chats.")
                .When(x => !x.IsGroup);

            RuleFor(x => x.MemberIds)
                .NotEmpty().WithMessage("At least one member is required for group chats.")
                .When(x => x.IsGroup);
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
                    new Error("Chat.Unauthenticated", "Not authenticated.", ErrorType.Unauthorized));

            var userId = _currentUser.UserId.Value;

            ChatRoom room;
            if (request.IsGroup)
            {
                room = ChatRoom.CreateGroup(userId, request.Name!, request.MemberIds!);
            }
            else
            {
                room = ChatRoom.CreateDirect(userId, request.OtherUserId!.Value);
            }

            _db.ChatRooms.Add(room);
            await _db.SaveChangesAsync(ct);

            return Result.Success(room.Id);
        }
    }
}