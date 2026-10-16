using ChatApp.API.Modules.Identity.Domain;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.API.Modules.Identity.Application.Commands;

public static class ResetPassword
{
    public record Command(
        string Email,
        string Token,
        string NewPassword) : IRequest<Result>;

    public class Validation : AbstractValidator<Command>
    {
        public Validation()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Reset token is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly UserManager<AppUser> _userManager;

        public Handler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Result.Failure(
                    new Error("Reset.InvalidRequest",
                        "Invalid reset request.",
                        ErrorType.Validation));
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!result.Succeeded)
            {
                var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                return Result.Failure(new Error("Reset.Failed", msg, ErrorType.Validation));
            }

            return Result.Success();
        }
    }
}