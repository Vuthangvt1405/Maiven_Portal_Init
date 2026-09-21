using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Services;
using Maiven_Portal_Managment.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/students")]
[Authorize(Roles = SystemRoles.Student.Code)]
public sealed class StudentController(
    UserService userService,
    CurrentUserContext currentUserContext) : ControllerBase
{
    [HttpGet("me")]
    public ActionResult<CurrentUserResponse> Me()
    {
        if (!currentUserContext.IsAuthenticated ||
            currentUserContext.UserId is not long userId ||
            string.IsNullOrWhiteSpace(currentUserContext.Email))
        {
            return Unauthorized();
        }

        return Ok(new CurrentUserResponse
        {
            Id = userId,
            Email = currentUserContext.Email,
            Roles = currentUserContext.Roles
        });
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserProfileResponse>> UpdateProfile(
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var response = await userService.UpdateCurrentStudentProfileAsync(
            request,
            cancellationToken);
        return Ok(response);
    }
}