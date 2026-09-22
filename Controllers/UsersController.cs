using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(UserService userService) : ControllerBase
{
    [HttpGet("me")]
    public ActionResult<CurrentUserResponse> Me()
    {
        var response = userService.GetCurrentUser();
        return Ok(response);
    }

    [HttpGet()]
    public async Task<ActionResult<IReadOnlyList<UserProfileResponse>>> GetAllUser(CancellationToken cancellationToken)
    {
        var response = await userService.GetAllUserAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("/api/admin/users/{userId:long}")]
    public async Task<ActionResult<UserProfileResponse>> GetUserById(
        long userId,
        CancellationToken cancellationToken)
    {
        var response = await userService.GetUserByIdAsync(userId, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("/api/admin/users/{userId:long}")]
    [Authorize(Roles = SystemRoles.Admin.Code)]
    public async Task<ActionResult> DeleteUserById(
        long userId,
        CancellationToken cancellationToken)
    {
        await userService.DeleteUserByIdAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserProfileResponse>> UpdateProfile(
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var response = await userService.UpdateCurrentUserProfileAsync(
            request,
            cancellationToken);
        return Ok(response);
    }
}
