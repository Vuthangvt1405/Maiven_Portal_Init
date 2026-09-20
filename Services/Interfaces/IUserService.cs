using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;

namespace Maiven_Portal_Managment.Services.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse> UpdateCurrentStudentProfileAsync(
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken);
}