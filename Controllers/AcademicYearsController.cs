using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api/academic-years")]
[Authorize(Roles = SystemRoles.Admin.Code)]
public sealed class AcademicYearsController(
    AcademicYearService academicYearService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AcademicYearResponse>> Create(
        [FromBody] CreateAcademicYearRequest request,
        CancellationToken cancellationToken)
    {
        var response = await academicYearService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AcademicYearResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await academicYearService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut("{academicYearId:long}")]
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
