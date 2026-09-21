
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Models.Enums;
using Maiven_Portal_Managment.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Services;

public sealed class CourseService(CourseRepository courseRepository)
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
        var model = BuildModel(
            request.CourseCode,
            request.CourseName,
            request.Credits,
            request.Description,
            request.Status);

        var existingCourse = await courseRepository.GetByCourseCodeAsync(
            model.CourseCode,
            cancellationToken);

        if (existingCourse is not null)
        {
            throw new ConflictException(DuplicateCodeMessage);
        }

        try
        {
            var created = await courseRepository.AddAsync(
                model,
                cancellationToken);

            return ToResponse(created);
        }
        catch (DbUpdateException exception)
        {
            var conflict = TranslateUniqueConstraintException(exception);

            if (conflict is not null)
            {
                throw conflict;
            }

            throw;
        }
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

        return ToResponse(course);
    }

    public async Task<CourseResponse> UpdateAsync(
        long courseId,
        UpdateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await courseRepository.GetByIdAsync(
            courseId,
            cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException(
                "The course could not be found.");
        }

        ValidateUpdateRequest(request);

        existing.CourseName = request.CourseName.Trim();
        existing.Credits = request.Credits;
        existing.Description = request.Description?.Trim();
        existing.Status = request.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        try
        {
            var updated = await courseRepository.UpdateAsync(
                existing,
                cancellationToken);

            if (updated is null)
            {
                throw new NotFoundException(
                    "The course could not be found.");
            }

            return ToResponse(updated);
        }
        catch (DbUpdateException exception)
        {
            var conflict = TranslateUniqueConstraintException(exception);

            if (conflict is not null)
            {
                throw conflict;
            }

            throw;
        }
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

    private static CourseModel BuildModel(
        string? courseCode,
        string? courseName,
        int? credits,
        string? description,
        ActiveStatus? status)
    {
        var normalizedCourseCode = courseCode?.Trim() ?? string.Empty;
        var normalizedCourseName = courseName?.Trim() ?? string.Empty;

        if (normalizedCourseCode.Length is < 1 or > 20)
        {
            throw new BadRequestException(
                "Course code must contain between 1 and 20 characters after trimming.");
        }

        if (normalizedCourseName.Length is < 1 or > 200)
        {
            throw new BadRequestException(
                "Course name must contain between 1 and 200 characters after trimming.");
        }

        if (!credits.HasValue)
        {
            throw new BadRequestException(
                "Credits is required.");
        }

        if (credits.Value <= 0)
        {
            throw new BadRequestException(
                InvalidCreditsMessage);
        }

        if (!status.HasValue)
        {
            throw new BadRequestException(
                "Status is required.");
        }

        if (!Enum.IsDefined(status.Value))
        {
            throw new BadRequestException(
                InvalidStatusMessage);
        }

        return new CourseModel
        {
            CourseCode = normalizedCourseCode,
            CourseName = normalizedCourseName,
            Credits = credits.Value,
            Description = description?.Trim(),
            Status = status.Value,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static void ValidateUpdateRequest(
        UpdateCourseRequest request)
    {
        var courseName = request.CourseName?.Trim();

        if (string.IsNullOrWhiteSpace(courseName))
        {
            throw new BadRequestException(
                "Course name is required.");
        }

        if (courseName.Length > 200)
        {
            throw new BadRequestException(
                "Course name must not exceed 200 characters.");
        }


        if (request.Credits <= 0)
        {
            throw new BadRequestException(
                InvalidCreditsMessage);
        }

        

        if (!Enum.IsDefined(request.Status))
        {
            throw new BadRequestException(
                InvalidStatusMessage);
        }
    }

    private static ConflictException? TranslateUniqueConstraintException(
        DbUpdateException exception)
    {
        if (exception.GetBaseException() is not SqlException sqlException ||
            sqlException.Number is not (2601 or 2627))
        {
            return null;
        }

        if (sqlException.Message.Contains(
                "UX_COURSES_course_code_not_deleted",
                StringComparison.OrdinalIgnoreCase))
        {
            return new ConflictException(
                DuplicateCodeMessage,
                exception);
        }

        return new ConflictException(
            "The course conflicts with an existing record.",
            exception);
    }

    private static CourseResponse ToResponse(
        CourseModel model) => new()
        {
            Id = model.Id,
            CourseCode = model.CourseCode,
            CourseName = model.CourseName,
            Credits = model.Credits,
            Description = model.Description,
            Status = model.Status,
            CreatedAt = AsUtc(model.CreatedAt),
            UpdatedAt = AsUtc(model.UpdatedAt)
        };

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
