using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/teachers")]
[Authorize(Roles = SystemRoles.Admin.Code)]
public sealed class TeachersController(TeacherService teacherService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<AuthUserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthUserResponse>> Create(
        [FromBody] CreateTeacherRequest request,
        CancellationToken cancellationToken)
    {
        var response = await teacherService.CreateTeacherAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
