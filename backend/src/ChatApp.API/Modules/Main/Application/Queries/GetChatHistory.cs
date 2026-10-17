using ChatApp.API.Modules.Main.Application.DTOs;
using ChatApp.API.Modules.Main.Application.Interfaces;
using ChatApp.API.Modules.Main.Infrastructure.Persistence;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Modules.Main.Application.Queries;

public static class GetChatHistory
{
    public record Query(
        Guid ChatRoomId,
        int Page = 1,
        int PageSize = 50) : IRequest<Result<ChatHistoryDto>>;

    public class Validation : AbstractValidator<Query>
    {
        public Validation()
        {
            RuleFor(x => x.ChatRoomId).NotEmpty();
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }

    public class Handler : IRequestHandler<Query, Result<ChatHistoryDto>>
    {
        private readonly MainDbContext _db;
        private readonly IChatHistoryRepository _history;
        private readonly ICurrentUser _currentUser;

        public Handler(MainDbContext db, IChatHistoryRepository history, ICurrentUser currentUser)
        {
            _db = db;
            _history = history;
            _currentUser = currentUser;
        }

        public async Task<Result<ChatHistoryDto>> Handle(Query request, CancellationToken ct)
        {
            if (_currentUser.UserId is null)
                return Result.Failure<ChatHistoryDto>(
                    new Error("History.Unauthenticated", "Not authenticated.", ErrorType.Unauthorized));

            var isMember = await _db.ChatRooms
                .AnyAsync(r => r.Id == request.ChatRoomId
                               && r.Members.Any(m => m.UserId == _currentUser.UserId), ct);

            if (!isMember)
                return Result.Failure<ChatHistoryDto>(
                    new Error("History.Forbidden", "You are not a member of this room.", ErrorType.Forbidden));

            var skip = (request.Page - 1) * request.PageSize;

            var messages = await _history.GetHistoryAsync(request.ChatRoomId, skip, request.PageSize, ct);
            var total = await _history.CountAsync(request.ChatRoomId, ct);

            var dtos = messages.Select(m => new MessageDto(
                m.Id,
                m.SenderUserId,
                m.Content,
                m.AttachmentId,
                m.MessageType,
                m.SentAt)).ToList();

            return Result.Success(new ChatHistoryDto(
                request.ChatRoomId, dtos, total, request.Page, request.PageSize));
        }
    }
}