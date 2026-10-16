using ChatApp.API.Modules.Identity.Application.Abstractions;
using ChatApp.API.Modules.Identity.Application.Models;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using FluentValidation;

namespace ChatApp.API.Modules.Identity.Application.Commands;

public static class RefreshToken
{
    public record Command(
        string RefreshToken,
        string? IpAddress = null) : IRequest<Result<AuthResult>>;

    public class Validation : AbstractValidator<Command>
    {
        public Validation()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.")
                .MaximumLength(500).WithMessage("Refresh token is invalid.");
        }
    }

    public class Handler : IRequestHandler<Command, Result<AuthResult>>
    {
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public Handler(ITokenService tokenService, IConfiguration configuration)
        {
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<Result<AuthResult>> Handle(
            Command request, CancellationToken cancellationToken)
        {
            var existing = await _tokenService.GetActiveRefreshTokenAsync(request.RefreshToken);

            if (existing is null)
            {
                return Result.Failure<AuthResult>(
                    new Error("Refresh.InvalidToken",
                        "Refresh token is invalid.",
                        ErrorType.Unauthorized));
            }

            if (existing.IsExpired)
            {
                return Result.Failure<AuthResult>(
                    new Error("Refresh.Expired",
                        "Refresh token has expired.",
                        ErrorType.Unauthorized));
            }

            var user = existing.User;

            var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user, request.IpAddress);

            await _tokenService.RevokeRefreshTokenAsync(existing, request.IpAddress, newRefreshToken.Token);

            var expiryMinutes = double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

            return Result.Success(new AuthResult(
                AccessToken: newAccessToken,
                RefreshToken: newRefreshToken.Token,
                AccessTokenExpiresAt: DateTimeOffset.UtcNow.AddMinutes(expiryMinutes),
                UserId: user.Id,
                UserName: user.UserName ?? string.Empty,
                DisplayName: user.DisplayName));
        }
    }
}