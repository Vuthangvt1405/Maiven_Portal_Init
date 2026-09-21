using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Services;
using Maiven_Portal_Managment.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = SystemRoles.Admin.Code)]
public sealed class AdminController(CurrentUserContext currentUserContext, SemesterService semesterService) : ControllerBase
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

    [HttpPost("semesters")]
    public async Task<ActionResult<CreateSemesterResponse>> CreateSemester([FromBody] CreateSemesterRequest request)
    {

        var response = await semesterService.CreateSemesterAsync(request);

        return Ok(response);
    }
}
