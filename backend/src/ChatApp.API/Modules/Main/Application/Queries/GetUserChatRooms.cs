using ChatApp.API.Modules.Main.Application.DTOs;
using ChatApp.API.Modules.Main.Infrastructure.Persistence;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Modules.Main.Application.Queries;

public static class GetUserChatRooms
{
    public record Query : IRequest<Result<IReadOnlyList<ChatRoomDto>>>;

    public class Handler : IRequestHandler<Query, Result<IReadOnlyList<ChatRoomDto>>>
    {
        private readonly MainDbContext _db;
        private readonly ICurrentUser _currentUser;

        public Handler(MainDbContext db, ICurrentUser currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<Result<IReadOnlyList<ChatRoomDto>>> Handle(Query request, CancellationToken ct)
        {
            if (_currentUser.UserId is null)
                return Result.Failure<IReadOnlyList<ChatRoomDto>>(
                    new Error("Rooms.Unauthenticated", "Not authenticated.", ErrorType.Unauthorized));

            var rooms = await _db.ChatRooms
                .Include(r => r.Members)
                .Where(r => r.Members.Any(m => m.UserId == _currentUser.UserId))
                .OrderByDescending(r => r.LastMessageAt)
                .Select(r => new ChatRoomDto(
                    r.Id,
                    r.Name,
                    r.IsGroup,
                    r.LastMessageAt,
                    r.Members.Select(m => m.UserId).ToList()))
                .ToListAsync(ct);

            return Result.Success<IReadOnlyList<ChatRoomDto>>(rooms);
        }
    }
}