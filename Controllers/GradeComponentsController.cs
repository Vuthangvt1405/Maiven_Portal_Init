using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/gradeComponents")]
[Authorize(Roles = SystemRoles.Admin.Code + "," + SystemRoles.Teacher.Code)]
public sealed class GradeComponentsController(GradeComponentService gradeComponentService) : ControllerBase
{
    [HttpPut("{courseSectionId:long}")]
    public async Task<ActionResult<IReadOnlyList<GradeComponentResponse>>> Update(
        long courseSectionId,
        [FromBody] UpdateGradeComponentsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await gradeComponentService.UpdateWeightsAsync(
            courseSectionId,
            request,
            cancellationToken);

        return Ok(response);
    }
}