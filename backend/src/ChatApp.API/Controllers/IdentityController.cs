using ChatApp.API.Modules.Identity.Application.Commands;
using ChatApp.API.Modules.Identity.Application.Queries;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IdentityController : BaseController
{
    private readonly ISender _sender;
    public IdentityController(ISender sender) => _sender = sender;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUser.Command command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(new { userId = result.Value }) : ToProblem(result.Error);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] Login.Command command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { IpAddress = IpAddress }, ct);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshToken.Command command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { IpAddress = IpAddress }, ct);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(
        [FromBody] Logout.Command command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { IpAddress = IpAddress }, ct);
        return result.IsSuccess ? NoContent() : ToProblem(result.Error);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPassword.Command command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPassword.Command command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : ToProblem(result.Error);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var result = await _sender.Send(new GetCurrentUser.Query(), ct);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }
}
