using ChatApp.API.Modules.Notification.Application.Abstractions;
using ChatApp.API.Modules.Notification.Application.Queries;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : BaseController
{
    private readonly ISender _sender;
    private readonly INotificationService _service;
    private readonly ICurrentUser _currentUser;

    public NotificationsController(
        ISender sender,
        INotificationService service,
        ICurrentUser currentUser)
    {
        _sender = sender;
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(
            new GetUserNotifications.Query(unreadOnly, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        await _service.MarkAsReadAsync(id, _currentUser.UserId.Value, ct);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        if (_currentUser.UserId is null) return Unauthorized();
        await _service.MarkAllAsReadAsync(_currentUser.UserId.Value, ct);
        return NoContent();
    }
}