using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/semesters")]
[Authorize(Roles = SystemRoles.Admin.Code)]
public sealed class SemestersController(SemesterService semesterService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<SemesterResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SemesterResponse>> Create(
        [FromBody] CreateSemesterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await semesterService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<SemesterResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<SemesterResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await semesterService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut("{semesterId:long}")]
    [ProducesResponseType<SemesterResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
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
