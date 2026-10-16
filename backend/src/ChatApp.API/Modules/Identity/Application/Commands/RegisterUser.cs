using ChatApp.API.Modules.Identity.Domain;
using ChatApp.API.Modules.Shared.Kernel.Constants;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.API.Modules.Identity.Application.Commands;

public static class RegisterUser
{
    public record Command(
        string Email,
        string Password,
        string? DisplayName) : IRequest<Result<Guid>>;

    public class Validation : AbstractValidator<Command>
    {
        public Validation()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .MaximumLength(128).WithMessage("Password must not exceed 128 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

            RuleFor(x => x.DisplayName)
                .MaximumLength(100).WithMessage("Display name must not exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));
        }
    }

    public class Handler : IRequestHandler<Command, Result<Guid>>
    {
        private readonly UserManager<AppUser> _userManager;

        public Handler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing is not null)
            {
                return Result.Failure<Guid>(
                    new Error("Registration.DuplicateEmail",
                        "A user with this email already exists.",
                        ErrorType.Conflict));
            }

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.DisplayName ?? request.Email.Split('@')[0]
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return Result.Failure<Guid>(
                    new Error("Registration.Failed",
                        string.Join("; ", result.Errors.Select(e => e.Description)),
                        ErrorType.Validation));
            }

            await _userManager.AddToRoleAsync(user, AppRoles.User);

            return Result.Success(user.Id);
        }
    }
}