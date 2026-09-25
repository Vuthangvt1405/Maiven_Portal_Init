using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Maiven_Portal_Managment.Services;
using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Services.Security;
using Maiven_Portal_Managment.Exceptions;


namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/teacher/courseSection")]
[Authorize(Roles = SystemRoles.Teacher.Code)]
public sealed class ChangeCourseSectionController(
    ChangeCourseSectionService changeCourseSectionService) : ControllerBase
{
    /// <summary>Marks a teacher-owned course section as completed.</summary>
    [HttpPatch("{sectionId:long}/completed")]
    public async Task<ActionResult<ChangeCourseSectionResponse>> ChangeCourseSectionComplete(
        long sectionId,
        CurrentUserContext currentTeacherContext,
        CancellationToken cancellationToken)
    {
        var teacherUserRoleId = currentTeacherContext.RoleUserId;

        if (teacherUserRoleId is null)
        {
            throw new UnauthorizedException("Teacher user role ID is not available.");
        }

        var response = await changeCourseSectionService.ChangeCourseSectionCompleteAsync(
            sectionId,
            teacherUserRoleId.Value,
            cancellationToken);

        return Ok(response);
    }
}
