using log4net;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Logging;
using Maiven_Portal_Managment.Repository;
using Maiven_Portal_Managment.Services.Security;

namespace Maiven_Portal_Managment.Services;

public sealed class UserService(
    UserRepository userRepository,
    CurrentUserContext currentUserContext)
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(UserService));

    public async Task<CurrentUserResponse> GetCurrentUser()
    {
        if (!currentUserContext.IsAuthenticated ||
            currentUserContext.UserId is not long userId ||
            string.IsNullOrWhiteSpace(currentUserContext.Email) ||
            string.IsNullOrWhiteSpace(currentUserContext.Role) ||
            currentUserContext.RoleUserId is not long roleUserId)
        {
            throw new UnauthorizedException("An authenticated user is required.");
        }

        var user = await userRepository.GetByIdAsync(userId, CancellationToken.None);

        if (user is null)
        {
            throw new NotFoundException("The active user account could not be found.");
        }

        var userRole = user.UserRoles.FirstOrDefault();
        var response = new CurrentUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = userRole?.Role.Code ?? currentUserContext.Role,
            RoleUserId = userRole?.Id ?? roleUserId,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Phone = user.Phone,
            Address = user.Address,
            AvatarUrl = user.AvatarUrl
        };
        Logger.Info(
            $"Current user retrieved UserId={response.Id} " +
            $"Role={LogFormat.FormatValue(response.Role)}");
        return response;
    }

    public async Task<IReadOnlyList<UserProfileResponse>> GetAllUserAsync(
        CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);

        var response = users.Select(ToUserProfileResponse).ToArray();
        Logger.Info($"Users retrieved ResultCount={response.Length}");
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
        Logger.Info(
            $"User retrieved UserId={response.Id} " +
            $"Email={LogFormat.FormatValue(response.Email)} " +
            $"Role={LogFormat.FormatValue(response.Role)}");
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
        Logger.Info($"User soft-deleted UserId={userId}");
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
        Logger.Info(
            $"User profile updated UserId={response.Id} " +
            $"Email={LogFormat.FormatValue(response.Email)}");
        return response;
    }

    public async Task<(IReadOnlyList<UserProfileResponse> Items, int TotalItems)> GetPagedUsersAsync(
        UserQueryParameters parameters,
         CancellationToken cancellationToken)
    {
        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);

        var result = await userRepository.GetPagedAsync(
            parameters.Name,
            parameters.Email,
            parameters.Role,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = result.Items.Select(ToUserProfileResponse).ToArray();
        Logger.Info(
            $"Users searched Name={LogFormat.FormatValue(parameters.Name ?? string.Empty)} " +
            $"Email={LogFormat.FormatValue(parameters.Email ?? string.Empty)} " +
            $"Role={LogFormat.FormatValue(parameters.Role?.ToString() ?? string.Empty)} " +
            $"PageNumber={pageNumber} PageSize={pageSize} " +
            $"ResultCount={items.Length} TotalItems={result.TotalItems}");
        return (items, result.TotalItems);
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
