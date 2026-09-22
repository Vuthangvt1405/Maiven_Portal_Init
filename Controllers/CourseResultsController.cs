using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maiven_Portal_Managment.Services;
using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Services.Security;
using Maiven_Portal_Managment.Exceptions;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/teacher/courseSections/{sectionId:long}/grades")]

public sealed class TeacherGradeController(CourseResultService courseResultService) : ControllerBase
{

    [HttpPatch("finalize")]
    [Authorize(Roles = SystemRoles.Teacher.Code)]
    public async Task<ActionResult> FinalizeGrades(
        [FromRoute] long sectionId,
        CurrentUserContext currentTeacher,
        CancellationToken cancellationToken)
    {
        if (currentTeacher.RoleUserId is not long teacherId)
        {
            throw new UnauthorizedException("User ID is not available.");
        }

        await courseResultService.FinalizeGradesAsync(sectionId, teacherId, cancellationToken);

        return Ok(new { Message = "Grades finalized successfully." });

    }
}