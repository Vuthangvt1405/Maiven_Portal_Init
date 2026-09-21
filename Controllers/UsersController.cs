using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
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
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserResponse> Me()
    {
        var response = userService.GetCurrentUser();
        return Ok(response);
    }

    [HttpPut("profile")]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
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
