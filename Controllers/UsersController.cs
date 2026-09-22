using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
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

    [HttpGet("/api/admin/users")]
    public async Task<ActionResult<PagedResponse<UserProfileResponse>>> GetAllUser(
        [FromQuery] UserQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetPagedUsersAsync(parameters, cancellationToken);
        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
      var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);

        var response = new PagedResponse<UserProfileResponse>
        {
         Items = result.Items,
        PageNumber = pageNumber,
         PageSize = pageSize,
         TotalItems = result.TotalItems,
        TotalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize)
        };

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
