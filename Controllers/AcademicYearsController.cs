using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/admin/academic-years")]
[Authorize(Roles = SystemRoles.Admin.Code)]
public sealed class AcademicYearsController(
    AcademicYearService academicYearService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<AcademicYearResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AcademicYearResponse>> Create(
        [FromBody] CreateAcademicYearRequest request,
        CancellationToken cancellationToken)
    {
        var response = await academicYearService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AcademicYearResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<AcademicYearResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await academicYearService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut("{academicYearId:long}")]
    [ProducesResponseType<AcademicYearResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AcademicYearResponse>> Update(
        long academicYearId,
        [FromBody] UpdateAcademicYearRequest request,
        CancellationToken cancellationToken)
    {
        var response = await academicYearService.UpdateAsync(
            academicYearId,
            request,
            cancellationToken);
        return Ok(response);
    }
}
