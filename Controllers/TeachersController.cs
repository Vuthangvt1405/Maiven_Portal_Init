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
[Route("api/teachers")]
public sealed class TeachersController(
    TeacherService teacherService,
    StudentScoreService studentScoreService) : ControllerBase
{
    /// <summary>Creates a teacher account.</summary>
    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin.Code)]
    public async Task<ActionResult<AuthUserResponse>> Create(
        [FromBody] CreateTeacherRequest request,
        CancellationToken cancellationToken)
    {
        var response = await teacherService.CreateTeacherAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Updates a student's score.</summary>
    [Authorize(Roles = SystemRoles.Teacher.Code)]
    [HttpPatch("/api/teachers/studentScores/{studentScoreId:long}")]
    public async Task<ActionResult<StudentScoreResponse>> UpdateStudentScore(
        long studentScoreId,
        [FromBody] UpdateStudentScoreRequest request,
        CancellationToken cancellationToken)
    {
        var response = await studentScoreService.UpdateAsync(
            studentScoreId,
            request,
            cancellationToken);

        return Ok(response);
    }
}
