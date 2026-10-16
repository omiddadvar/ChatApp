using ChatApp.API.Modules.Identity.Application.Abstractions;
using ChatApp.API.Modules.Identity.Application.Models;
using ChatApp.API.Modules.Identity.Domain;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.API.Modules.Identity.Application.Commands;

public static class Login
{
    public record Command(
        string Email,
        string Password,
        string? IpAddress = null) : IRequest<Result<AuthResult>>;

    public class Validation : AbstractValidator<Command>
    {
        public Validation()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }

    public class Handler : IRequestHandler<Command, Result<AuthResult>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public Handler(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<Result<AuthResult>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Result.Failure<AuthResult>(
                    new Error("Login.InvalidCredentials",
                        "Email or password is incorrect.",
                        ErrorType.Unauthorized));
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return Result.Failure<AuthResult>(
                    new Error("Login.LockedOut",
                        "Account is temporarily locked. Please try again later.",
                        ErrorType.Forbidden));
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                await _userManager.AccessFailedAsync(user);
                return Result.Failure<AuthResult>(
                    new Error("Login.InvalidCredentials",
                        "Email or password is incorrect.",
                        ErrorType.Unauthorized));
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, request.IpAddress);

            var expiryMinutes = double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

            return Result.Success(new AuthResult(
                AccessToken: accessToken,
                RefreshToken: refreshToken.Token,
                AccessTokenExpiresAt: DateTimeOffset.UtcNow.AddMinutes(expiryMinutes),
                UserId: user.Id,
                UserName: user.UserName ?? string.Empty,
                DisplayName: user.DisplayName));
        }
    }
}