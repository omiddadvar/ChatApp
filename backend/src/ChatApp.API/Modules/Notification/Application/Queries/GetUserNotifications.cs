using ChatApp.API.Modules.Notification.Application.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;

namespace ChatApp.API.Modules.Notification.Application.Queries;

public static class GetUserNotifications
{
    public record Query(
        bool UnreadOnly = false,
        int Page = 1,
        int PageSize = 50) : IRequest<Result<NotificationsDto>>;

    public record NotificationsDto(
        IReadOnlyList<NotificationDto> Items,
        int TotalCount,
        int UnreadCount);

    public class Handler : IRequestHandler<Query, Result<NotificationsDto>>
    {
        private readonly INotificationService _service;
        private readonly ICurrentUser _currentUser;

        public Handler(INotificationService service, ICurrentUser currentUser)
        {
            _service = service;
            _currentUser = currentUser;
        }

        public async Task<Result<NotificationsDto>> Handle(
            Query request,
            CancellationToken ct)
        {
            if (_currentUser.UserId is null)
                return Result.Failure<NotificationsDto>(
                    new Error("Notifications.Unauthenticated", "Not authenticated.", ErrorType.Unauthorized));

            var skip = (request.Page - 1) * request.PageSize;
            var items = await _service.GetUserNotificationsAsync(
                _currentUser.UserId.Value, request.UnreadOnly, skip, request.PageSize, ct);

            var unread = await _service.GetUnreadCountAsync(_currentUser.UserId.Value, ct);

            var dtos = items.Select(n => new NotificationDto(
                n.Id, n.Type, n.Title, n.Body, n.PayloadJson, n.IsRead, n.CreatedAt)).ToList();

            return Result.Success(new NotificationsDto(dtos, dtos.Count, unread));
        }
    }
}