
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Data.Entities.Enums;
using Maiven_Portal_Managment.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Services;

public sealed class CourseService(
    CourseRepository courseRepository)
{
    private const string DuplicateCodeMessage =
        "A course with the same code already exists.";

    private const string InvalidCreditsMessage =
        "Course credits must be greater than 0.";

    private const string InvalidStatusMessage =
        "Course status is invalid.";

    public async Task<CourseResponse> CreateAsync(
        CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var existingCourse = await courseRepository.GetByCourseCodeAsync(
            request.CourseCode,
            cancellationToken);

        if (existingCourse is not null)
        {
            throw new ConflictException(DuplicateCodeMessage);
        }

        var entity = BuildEntity(
            request.CourseCode,
            request.CourseName,
            request.Credits,
            request.Description ?? string.Empty,
            ActiveStatus.ACTIVE
           );

        var created = await courseRepository.AddAsync(
            entity,
            cancellationToken);

        var response = ToResponse(created);
        return response;
    }

    public async Task<(IReadOnlyList<CourseResponse> Items, int TotalItems)> GetPagedAsync(
        CourseQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var pageNumber = parameters.PageNumber < 1
            ? 1
            : parameters.PageNumber;

        var pageSize = parameters.PageSize < 1
            ? 10
            : Math.Min(parameters.PageSize, 100);

        var result = await courseRepository.GetPagedAsync(
            parameters.CourseCode,
            parameters.CourseName,
            parameters.Credits,
            parameters.Status,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = result.Items
            .Select(ToResponse)
            .ToArray();

        return (items, result.TotalItems);
    }

    public async Task<CourseResponse> GetByIdAsync(
        long courseId,
        CancellationToken cancellationToken)
    {
        var course = await courseRepository.GetByIdAsync(
            courseId,
            cancellationToken);

        if (course is null)
        {
            throw new NotFoundException(
                "The course could not be found.");
        }

        var response = ToResponse(course);
        return response;
    }

    public async Task<CourseResponse> UpdateAsync(
        long courseId,
        UpdateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await courseRepository.GetTrackedByIdAsync(
            courseId,
            cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException(
                "The course could not be found.");
        }

        existing.CourseName = request.CourseName.Trim();
        existing.Credits = request.Credits;
        existing.Description = request.Description?.Trim();
        existing.Status = request.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        await courseRepository.SaveChangesAsync(cancellationToken);

        var response = ToResponse(existing);
        return response;
    }

    public async Task DeleteAsync(
        long courseId,
        CancellationToken cancellationToken)
    {
        var deleted = await courseRepository.DeleteAsync(
            courseId,
            cancellationToken);

        if (!deleted)
        {
            throw new NotFoundException(
                "The course could not be found.");
        }
    }

    private static Course BuildEntity(
        string courseCode,
        string courseName,
        int credits,
        string description,
        ActiveStatus status)
    {
        var normalizedCourseCode = courseCode?.Trim() ?? string.Empty;
        var normalizedCourseName = courseName?.Trim() ?? string.Empty;

        return new Course
        {
            CourseCode = normalizedCourseCode,
            CourseName = normalizedCourseName,
            Credits = credits,
            Description = description?.Trim(),
            Status = status,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }


    private static CourseResponse ToResponse(Course entity) => new(
        Id: entity.Id,
        CourseCode: entity.CourseCode,
        CourseName: entity.CourseName,
        Credits: entity.Credits,
        Description: entity.Description,
        Status: entity.Status,
        CreatedAt: AsUtc(entity.CreatedAt),
        UpdatedAt: AsUtc(entity.UpdatedAt)
    );

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
