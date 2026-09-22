using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Common;
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
    public async Task<ActionResult<AuthUserResponse>> Create(
        [FromBody] CreateTeacherRequest request,
        CancellationToken cancellationToken)
    {
        var response = await teacherService.CreateTeacherAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
