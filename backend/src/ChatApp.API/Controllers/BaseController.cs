using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.API.Controllers;

public abstract class BaseController : ControllerBase
{
    protected string? IpAddress
        => HttpContext.Connection.RemoteIpAddress?.ToString();

    protected ObjectResult ToProblem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(statusCode: statusCode, title: error.Code, detail: error.Message);
    }
}
