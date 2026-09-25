using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/semesters")]
public sealed class SemestersController(SemesterService semesterService) : ControllerBase
{
    /// <summary>Creates a semester.</summary>
    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin.Code)]
    public async Task<ActionResult<SemesterResponse>> Create(
        [FromBody] CreateSemesterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await semesterService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Retrieves all semesters.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<SemesterResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await semesterService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>Updates a semester.</summary>
    [HttpPut("{semesterId:long}")]
    [Authorize(Roles = SystemRoles.Admin.Code)]
    public async Task<ActionResult<SemesterResponse>> Update(
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
