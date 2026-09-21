using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Logging;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Repository;
using Maiven_Portal_Managment.Services.Security;

namespace Maiven_Portal_Managment.Services;

public sealed class UserService(
    UserRepository userRepository,
    CurrentUserContext currentUserContext,
    ActionLogService actionLogService)
{
    public CurrentUserResponse GetCurrentUser()
    {
        using var operation = actionLogService.Begin<UserService>(
            "Service",
            nameof(GetCurrentUser));

        if (!currentUserContext.IsAuthenticated ||
            currentUserContext.UserId is not long userId ||
            string.IsNullOrWhiteSpace(currentUserContext.Email) ||
            string.IsNullOrWhiteSpace(currentUserContext.Role) ||
            currentUserContext.RoleUserId is not long roleUserId)
        {
            throw new UnauthorizedException("An authenticated user is required.");
        }

        var response = new CurrentUserResponse
        {
            Id = userId,
            Email = currentUserContext.Email,
            Role = currentUserContext.Role,
            RoleUserId = roleUserId
        };
        operation.Complete(("UserId", response.Id), ("Role", response.Role));
        return response;
    }

    public async Task<IReadOnlyList<UserProfileResponse>> GetAllUserAsync(
        CancellationToken cancellationToken)
    {
        using var operation = actionLogService.Begin<UserService>(
            "Service",
            nameof(GetAllUserAsync));
        var users = await userRepository.GetAllAsync(cancellationToken);

        var response = users.Select(user => new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            RoleUserId = user.RoleUserId,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Phone = user.Phone,
            Address = user.Address,
            AvatarUrl = user.AvatarUrl
        }).ToArray();
        operation.Complete(("ResultCount", response.Length));
        return response;
    }

    public async Task<UserProfileResponse> GetUserByIdAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        using var operation = actionLogService.Begin<UserService>(
            "Service",
            nameof(GetUserByIdAsync),
            ("TargetUserId", userId));
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("The user account could not be found.");
        }

        var response = ToUserProfileResponse(user);
        operation.Complete(("TargetUserId", response.Id));
        return response;
    }

    public async Task DeleteUserByIdAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        using var operation = actionLogService.Begin<UserService>(
            "Service",
            nameof(DeleteUserByIdAsync),
            ("TargetUserId", userId));
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("The user account could not be found.");
        }

        await userRepository.DeleteByIdAsync(userId, cancellationToken);
        operation.Complete(("TargetUserId", userId));
    }

    public async Task<UserProfileResponse> UpdateCurrentUserProfileAsync(
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        using var operation = actionLogService.Begin<UserService>(
            "Service",
            nameof(UpdateCurrentUserProfileAsync));

        if (!currentUserContext.IsAuthenticated ||
            currentUserContext.UserId is not long userId)
        {
            throw new UnauthorizedException("An authenticated user is required.");
        }

        var profile = new UserModel
        {
            FullName = request.FullName.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Phone = NormalizeOptional(request.Phone),
            Address = NormalizeOptional(request.Address),
            AvatarUrl = NormalizeOptional(request.AvatarUrl)
        };

        var updatedUser = await userRepository.UpdateProfileAsync(
            userId,
            profile,
            cancellationToken);

        if (updatedUser is null)
        {
            throw new NotFoundException("The active user account could not be found.");
        }

        var response = ToUserProfileResponse(updatedUser);
        operation.Complete(("UserId", response.Id));
        return response;
    }

    private static UserProfileResponse ToUserProfileResponse(UserModel user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FullName = user.FullName,
        Role = user.Role,
        RoleUserId = user.RoleUserId,
        DateOfBirth = user.DateOfBirth,
        Gender = user.Gender,
        Phone = user.Phone,
        Address = user.Address,
        AvatarUrl = user.AvatarUrl
    };

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
