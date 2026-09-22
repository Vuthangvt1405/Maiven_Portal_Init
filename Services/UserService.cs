using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Repository;
using Maiven_Portal_Managment.Services.Security;

namespace Maiven_Portal_Managment.Services;

public sealed class UserService(
    UserRepository userRepository,
    CurrentUserContext currentUserContext)
{
    public CurrentUserResponse GetCurrentUser()
    {
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
        return response;
    }

    public async Task<IReadOnlyList<UserProfileResponse>> GetAllUserAsync(
        CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);

        var response = users.Select(ToUserProfileResponse).ToArray();
        return response;
    }

    public async Task<UserProfileResponse> GetUserByIdAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("The user account could not be found.");
        }

        var response = ToUserProfileResponse(user);
        return response;
    }

    public async Task DeleteUserByIdAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("The user account could not be found.");
        }

        await userRepository.DeleteByIdAsync(userId, cancellationToken);
    }

    public async Task<UserProfileResponse> UpdateCurrentUserProfileAsync(
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!currentUserContext.IsAuthenticated ||
            currentUserContext.UserId is not long userId)
        {
            throw new UnauthorizedException("An authenticated user is required.");
        }

        var user = await userRepository.GetTrackedByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("The active user account could not be found.");
        }

        user.FullName = request.FullName.Trim();
        user.DateOfBirth = request.DateOfBirth;
        user.Gender = request.Gender;
        user.Phone = NormalizeOptional(request.Phone);
        user.Address = NormalizeOptional(request.Address);
        user.AvatarUrl = NormalizeOptional(request.AvatarUrl);
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.SaveChangesAsync(cancellationToken);

        var response = ToUserProfileResponse(user);
        return response;
    }

    private static UserProfileResponse ToUserProfileResponse(User user)
    {
        var userRole = user.UserRoles.FirstOrDefault();

        return new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = userRole?.Role.Code ?? string.Empty,
            RoleUserId = userRole?.Id ?? 0,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Phone = user.Phone,
            Address = user.Address,
            AvatarUrl = user.AvatarUrl
        };
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
