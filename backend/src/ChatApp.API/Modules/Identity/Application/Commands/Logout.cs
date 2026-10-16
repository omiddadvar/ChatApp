using ChatApp.API.Modules.Identity.Application.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using FluentValidation;

namespace ChatApp.API.Modules.Identity.Application.Commands;

public static class Logout
{
    public record Command(
        string? RefreshToken,
        bool RevokeAll = false,
        string? IpAddress = null) : IRequest<Result>;

    public class Validation : AbstractValidator<Command>
    {
        public Validation()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("Refresh token is required unless RevokeAll is true.")
                .When(x => !x.RevokeAll);

            RuleFor(x => x.RefreshToken)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.RefreshToken));
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ITokenService _tokenService;
        private readonly ICurrentUser _currentUser;

        public Handler(ITokenService tokenService, ICurrentUser currentUser)
        {
            _tokenService = tokenService;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.RevokeAll)
            {
                if (_currentUser.UserId is null)
                {
                    return Result.Failure(
                        new Error("Logout.Unauthenticated",
                            "Not authenticated.",
                            ErrorType.Unauthorized));
                }

                await _tokenService.RevokeAllUserTokensAsync(_currentUser.UserId.Value, request.IpAddress);
                return Result.Success();
            }

            var token = await _tokenService.GetActiveRefreshTokenAsync(request.RefreshToken!);
            if (token is null)
            {
                return Result.Success();
            }

            await _tokenService.RevokeRefreshTokenAsync(token, request.IpAddress);
            return Result.Success();
        }
    }
}