using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api")]
public sealed class CourseSectionsController(CourseSectionService courseSectionService) : ControllerBase
{

    [HttpGet ("course-sections")]
    [ProducesResponseType<PagedResponse<CourseSectionResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<CourseSectionResponse>>> GetAll(
        [FromQuery] CourseSectionQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await courseSectionService.GetPagedAsync(parameters, cancellationToken);

        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);
        var totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

        var response = new PagedResponse<CourseSectionResponse>
        {
            Items = result.Items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = result.TotalItems,
            TotalPages = totalPages
        };

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin.Code)]
    public async Task<ActionResult<CourseSectionResponse>> Create(
        [FromBody] CreateCourseSectionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await courseSectionService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(AdminGetById),
            new { courseSectionId = response.Id },
            response);
    }

    [Authorize(Roles = SystemRoles.Admin.Code)]
    [HttpGet("course-secions/{courseSectionId:long}")]
    public async Task<ActionResult<CourseSectionResponse>> AdminGetById(
        long courseSectionId,
        CancellationToken cancellationToken)
    {
        var response = await courseSectionService.GetByIdAsync(courseSectionId, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = SystemRoles.Teacher.Code)]
    [HttpGet("teacher/me/course-secions")]
    public async Task<ActionResult<CourseSectionResponse>> TeacherGetById(
        long courseSectionId,
        CancellationToken cancellationToken)
    {
        var response = await courseSectionService.GetByIdAsync(courseSectionId, cancellationToken);
        return Ok(response);
    }

    [Authorize(Roles = SystemRoles.Student.Code)]
    [HttpGet("student/me/course-secions")]
    public async Task<ActionResult<CourseSectionResponse>> StudentGetById(
        long courseSectionId,
        CancellationToken cancellationToken)
    {
        var response = await courseSectionService.GetByIdAsync(courseSectionId, cancellationToken);
        return Ok(response);
    }


    [HttpPut("{courseSectionId:long}")]
    [Authorize(Roles = SystemRoles.Admin.Code)]
    public async Task<ActionResult<CourseSectionResponse>> Update(
        long courseSectionId,
        [FromBody] UpdateCourseSectionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await courseSectionService.UpdateAsync(
            courseSectionId,
            request,
            cancellationToken);

        return Ok(response);
    }
}