using ChatApp.API.Modules.Identity.Application.Models;
using ChatApp.API.Modules.Identity.Domain;
using ChatApp.API.Modules.Shared.Kernel.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.Mediator.Abstractions;
using ChatApp.API.Modules.Shared.Kernel.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.API.Modules.Identity.Application.Queries;

public static class GetCurrentUser
{
    public record Query : IRequest<Result<UserProfileDto>>;

    public class Handler : IRequestHandler<Query, Result<UserProfileDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICurrentUser _currentUser;

        public Handler(UserManager<AppUser> userManager, ICurrentUser currentUser)
        {
            _userManager = userManager;
            _currentUser = currentUser;
        }

        public async Task<Result<UserProfileDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is null)
            {
                return Result.Failure<UserProfileDto>(
                    new Error("User.Unauthenticated",
                        "Not authenticated.",
                        ErrorType.Unauthorized));
            }

            var user = await _userManager.FindByIdAsync(_currentUser.UserId.Value.ToString());
            if (user is null)
            {
                return Result.Failure<UserProfileDto>(
                    new Error("User.NotFound",
                        "User not found.",
                        ErrorType.NotFound));
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Result.Success(new UserProfileDto(
                user.Id,
                user.Email ?? string.Empty,
                user.UserName ?? string.Empty,
                user.DisplayName,
                user.AvatarUrl,
                roles,
                user.CreatedAt));
        }
    }
}