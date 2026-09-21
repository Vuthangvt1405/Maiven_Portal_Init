using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Models.Enums;
using Maiven_Portal_Managment.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Services;

public sealed class CourseSectionService(CourseSectionRepository courseSectionRepository)
{
    private const string DuplicateSectionCodeMessage =
        "A course section with the same code already exists in this semester.";

    private const string InvalidTimeRangeMessage =
        "Start time must be before end time.";

    private const string InvalidDateRangeMessage =
        "Start date must be before or equal to end date.";

    private const string InvalidCapacityMessage =
        "Capacity must be greater than 0.";

    public async Task<CourseSectionResponse> CreateAsync(
        CreateCourseSectionRequest request,
        CancellationToken cancellationToken)
    {
        ValidateScheduleAndCapacity(
            request.StartTime,
            request.EndTime,
            request.StartDate,
            request.EndDate,
            request.Capacity);

        var normalizedSectionCode = request.SectionCode?.Trim() ?? string.Empty;
        if (normalizedSectionCode.Length is < 1 or > 50)
        {
            throw new BadRequestException("Section code must contain between 1 and 50 characters.");
        }

        var existingSection = await courseSectionRepository.GetBySemesterAndCodeAsync(
            request.SemesterId,
            normalizedSectionCode,
            cancellationToken);

        if (existingSection is not null)
        {
            throw new ConflictException(DuplicateSectionCodeMessage);
        }

        var model = new CourseSectionModel
        {
            CourseId = request.CourseId,
            SemesterId = request.SemesterId,
            TeacherUserRoleId = request.TeacherUserRoleId,
            SectionCode = normalizedSectionCode,
            Capacity = request.Capacity,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            var created = await courseSectionRepository.AddAsync(model, cancellationToken);
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

    public async Task<(IReadOnlyList<CourseSectionResponse> Items, int TotalItems)> GetPagedAsync(
        CourseSectionQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);

        var result = await courseSectionRepository.GetPagedAsync(
            parameters.CourseId,
            parameters.SemesterId,
            parameters.TeacherUserRoleId,
            parameters.SectionCode,
            parameters.DayOfWeek,
            parameters.Status,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = result.Items
            .Select(ToResponse)
            .ToArray();

        return (items, result.TotalItems);
    }

    public async Task<CourseSectionResponse> GetByIdAsync(
        long sectionId,
        CancellationToken cancellationToken)
    {
        var section = await courseSectionRepository.GetByIdAsync(sectionId, cancellationToken);
        if (section is null)
        {
            throw new NotFoundException("The course section could not be found.");
        }

        return ToResponse(section);
    }

    public async Task<CourseSectionResponse> UpdateAsync(
        long sectionId,
        UpdateCourseSectionRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await courseSectionRepository.GetByIdAsync(sectionId, cancellationToken);
        if (existing is null)
        {
            throw new NotFoundException("The course section could not be found.");
        }

        ValidateScheduleAndCapacity(
            request.StartTime,
            request.EndTime,
            request.StartDate,
            request.EndDate,
            request.Capacity);

        if (request.TeacherUserRoleId.HasValue)
        {
            existing.TeacherUserRoleId = request.TeacherUserRoleId.Value;
        }

        existing.Capacity = request.Capacity;
        existing.DayOfWeek = request.DayOfWeek;
        existing.StartTime = request.StartTime;
        existing.EndTime = request.EndTime;
        existing.StartDate = request.StartDate;
        existing.EndDate = request.EndDate;
        existing.Status = request.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        try
        {
            var updated = await courseSectionRepository.UpdateAsync(existing, cancellationToken);
            if (updated is null)
            {
                throw new NotFoundException("The course section could not be found.");
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

    private static void ValidateScheduleAndCapacity(
        TimeOnly startTime,
        TimeOnly endTime,
        DateOnly startDate,
        DateOnly endDate,
        int capacity)
    {
        if (capacity <= 0)
        {
            throw new BadRequestException(InvalidCapacityMessage);
        }

        if (startTime >= endTime)
        {
            throw new BadRequestException(InvalidTimeRangeMessage);
        }

        if (startDate > endDate)
        {
            throw new BadRequestException(InvalidDateRangeMessage);
        }
    }

    private static ConflictException? TranslateUniqueConstraintException(DbUpdateException exception)
    {
        if (exception.GetBaseException() is not SqlException sqlException ||
            sqlException.Number is not (2601 or 2627))
        {
            return null;
        }

        if (sqlException.Message.Contains("UX_COURSE_SECTIONS", StringComparison.OrdinalIgnoreCase))
        {
            return new ConflictException(DuplicateSectionCodeMessage, exception);
        }

        return new ConflictException("The course section conflicts with an existing record.", exception);
    }

    private static CourseSectionResponse ToResponse(CourseSectionModel model) => new()
    {
        Id = model.Id,
        CourseId = model.CourseId,
        SemesterId = model.SemesterId,
        TeacherUserRoleId = model.TeacherUserRoleId,
        SectionCode = model.SectionCode,
        Capacity = model.Capacity,
        DayOfWeek = model.DayOfWeek,
        StartTime = model.StartTime,
        EndTime = model.EndTime,
        StartDate = model.StartDate,
        EndDate = model.EndDate,
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