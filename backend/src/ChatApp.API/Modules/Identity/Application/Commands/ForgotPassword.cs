using ChatApp.API.Modules.Identity.Domain;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.API.Modules.Identity.Application.Commands;

public static class ForgotPassword
{
    public record Command(string Email) : IRequest<Result<ResultDto>>;

    public record ResultDto(string? ResetToken);

    public class Validation : AbstractValidator<Command>
    {
        public Validation()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");
        }
    }

    public class Handler : IRequestHandler<Command, Result<ResultDto>>
    {
        private readonly UserManager<AppUser> _userManager;

        public Handler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<ResultDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return Result.Success(new ResultDto(null));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return Result.Success(new ResultDto(token));
        }
    }
}