using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Services;
using Maiven_Portal_Managment.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maiven_Portal_Managment.Controllers;

[ApiController]
[Route("api")]
public sealed class CourseSectionsController(CourseSectionService courseSectionService) : ControllerBase
{

    [HttpGet("course-sections")]
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

    [HttpPost("course-sections")]
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
    [HttpGet("teachers/me/course-sections")]
    public async Task<ActionResult<PagedResponse<CourseSectionResponse>>> TeacherGetAll(
        [FromQuery] CourseSectionQueryParameters parameters,
        CurrentUserContext currentTeacherContext,
        CancellationToken cancellationToken)
    {
        var teacherUserRoleId = currentTeacherContext.RoleUserId;
        if (teacherUserRoleId is null)
        {
            return BadRequest("Invalid teacher user role ID.");
        }

        var result = await courseSectionService.TeacherGetPagedAsync(
            teacherUserRoleId,
            parameters,
            cancellationToken);

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
    [Authorize(Roles = SystemRoles.Teacher.Code)]
    [HttpGet("teachers/me/course-sections/{courseSectionId:long}")]
    public async Task<ActionResult<CourseSectionDetailResponse>> TeacherGetSectionDetail(
    [FromRoute] long courseSectionId,
    [FromQuery] PaginationQueryParameters parameters,
    CurrentUserContext currentTeacherContext,
    CancellationToken cancellationToken)
    {
        var teacherUserRoleId = currentTeacherContext.RoleUserId;
        if (teacherUserRoleId is null)
        {
            return BadRequest("Invalid teacher user role ID.");
        }

        var result = await courseSectionService.TeacherGetCourseSectionDetailsAsync(
            teacherUserRoleId.Value,
            courseSectionId,
            parameters,
            cancellationToken);

        if (result == null)
        {
            return NotFound("Course section not found or you do not have permission to view it.");
        }

        return Ok(result);
    }

    [Authorize(Roles = SystemRoles.Student.Code)]
    [HttpGet("student/me/course-sections")]
    public async Task<ActionResult<PagedResponse<CourseSectionResponse>>> StudentGetAll(
        [FromQuery] CourseSectionQueryParameters parameters,
        CurrentUserContext currentStudentContext,
        CancellationToken cancellationToken)
    {
        var studentUserRoleId = currentStudentContext.RoleUserId;
        if (studentUserRoleId is null)
        {
            return BadRequest("Invalid student user role ID.");
        }

        var result = await courseSectionService.StudentGetPagedAsync(
            studentUserRoleId,
            parameters,
            cancellationToken);

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

    [Authorize(Roles = SystemRoles.Student.Code)]
    [HttpGet("student/me/course-results")]
    public async Task<ActionResult<PagedResponse<StudentCourseResultResponse>>> StudentGetResults(
        [FromQuery] StudentCourseResultQueryParameters parameters,
        CurrentUserContext currentStudentContext,
        CancellationToken cancellationToken)
    {
        var studentUserRoleId = currentStudentContext.RoleUserId;
        if (studentUserRoleId is null)
        {
            return BadRequest("Invalid student user role ID.");
        }

        var result = await courseSectionService.StudentGetResultsAsync(
            studentUserRoleId,
            parameters,
            cancellationToken);

        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);
        var totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

        return Ok(new PagedResponse<StudentCourseResultResponse>
        {
            Items = result.Items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = result.TotalItems,
            TotalPages = totalPages
        });
    }


    [HttpPut("course-sections/{courseSectionId:long}")]
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