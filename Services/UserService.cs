using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Repository;
using Maiven_Portal_Managment.Services.Security;

namespace Maiven_Portal_Managment.Services;

public sealed class UserService(
    UserRepository userRepository,
    CurrentUserContext currentUserContext)
{
    public async Task<UserProfileResponse> UpdateCurrentStudentProfileAsync(
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
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

        var updatedUser = await userRepository.UpdateStudentProfileAsync(
            userId,
            profile,
            cancellationToken);

        if (updatedUser is null)
        {
            throw new NotFoundException("The active student account could not be found.");
        }

        return new UserProfileResponse
        {
            Id = updatedUser.Id,
            Email = updatedUser.Email,
            FullName = updatedUser.FullName,
            DateOfBirth = updatedUser.DateOfBirth,
            Gender = updatedUser.Gender,
            Phone = updatedUser.Phone,
            Address = updatedUser.Address,
            AvatarUrl = updatedUser.AvatarUrl
        };
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}