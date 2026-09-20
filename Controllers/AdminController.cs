using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = SystemRoles.Admin.Code)]
public sealed class AdminController(CurrentUserContext currentUserContext) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
}
