using ChatApp.API.Modules.Notification.Application.Abstractions;
using ChatApp.API.Modules.Notification.Domain.Enums;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using FluentValidation;

namespace ChatApp.API.Modules.Notification.Application.Commands;

public static class CreateNotification
{
    public record Command(
        Guid UserId,
        NotificationType Type,
        string Title,
        string Body,
        string? PayloadJson = null) : IRequest<Result<Guid>>;

    public class Validation : AbstractValidator<Command>
    {
        public Validation()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Type).IsInEnum().Must(t => t != NotificationType.Unknown);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Body).MaximumLength(2000);
        }
    }

    public class Handler : IRequestHandler<Command, Result<Guid>>
    {
        private readonly INotificationService _service;
        private readonly INotificationDispatcher _dispatcher;

        public Handler(
            INotificationService service,
            INotificationDispatcher dispatcher)
        {
            _service = service;
            _dispatcher = dispatcher;
        }

        public async Task<Result<Guid>> Handle(Command request, CancellationToken ct)
        {
            var id = await _service.CreateAsync(
                request.UserId, request.Type, request.Title,
                request.Body, request.PayloadJson, ct);

            var notification = (await _service.GetUserNotificationsAsync(
                request.UserId, take: 1, ct: ct)).FirstOrDefault();

            if (notification is not null)
                await _dispatcher.DispatchAsync(notification, ct);

            return Result.Success(id);
        }
    }
}