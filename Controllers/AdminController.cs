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
public sealed class AdminController(
    CurrentUserContext currentUserContext,
    TeacherService teacherService,SemesterService semesterService) : ControllerBase
{
    [HttpPost("teachers")]
    [ProducesResponseType<AuthUserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthUserResponse>> CreateTeacher(
        [FromBody] CreateTeacherRequest request,
        CancellationToken cancellationToken)
    {
        var response = await teacherService.CreateTeacherAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

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
    public async Task<ActionResult<SemesterResponse>> CreateSemester(
        [FromBody] CreateSemesterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await semesterService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet("semesters")]
    public async Task<ActionResult<IReadOnlyList<SemesterResponse>>> GetSemesters(
        CancellationToken cancellationToken)
    {
        var response = await semesterService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut("semesters/{semesterId:long}")]
    public async Task<ActionResult<SemesterResponse>> UpdateSemester(
        long semesterId,
        [FromBody] UpdateSemesterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await semesterService.UpdateAsync(
            semesterId,
            request,
            cancellationToken);
        return Ok(response);
    }
}
