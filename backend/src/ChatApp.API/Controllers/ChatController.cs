using ChatApp.API.Modules.Main.Application.Commands;
using ChatApp.API.Modules.Main.Application.Queries;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : BaseController
{
    private readonly ISender _sender;
    public ChatController(ISender sender) => _sender = sender;

    [HttpPost("rooms")]
    public async Task<IActionResult> CreateRoom(
        [FromBody] CreateChatRoom.Command command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : ToProblem(result.Error);
    }

    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms(CancellationToken ct)
    {
        var result = await _sender.Send(new GetUserChatRooms.Query(), ct);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    [HttpGet("rooms/{roomId:guid}/messages")]
    public async Task<IActionResult> GetHistory(
        Guid roomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(
            new GetChatHistory.Query(roomId, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage(
        [FromBody] SendMessage.Command command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : ToProblem(result.Error);
    }

    [HttpPost("attachments")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadAttachment(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var command = new UploadAttachment.Command(
            stream, file.FileName, file.ContentType, file.Length);

        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : ToProblem(result.Error);
    }

    [HttpPost("contacts")]
    public async Task<IActionResult> AddContact(
        [FromBody] AddContact.Command command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(new { id = result.Value }) : ToProblem(result.Error);
    }
}