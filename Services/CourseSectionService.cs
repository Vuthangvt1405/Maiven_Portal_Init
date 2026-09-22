using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Data.Entities.Enums;
using Maiven_Portal_Managment.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Services;

public sealed class CourseSectionService(
    CourseSectionRepository courseSectionRepository,
    SemesterRepository semesterRepository,
    UserRepository userRepository,
    CourseRepository courseRepository)
{
    private const string DuplicateSectionCodeMessage =
        "A course section with the same code already exists in this semester.";

    private const string InvalidTimeRangeMessage =
        "Start period must be between 1 and 10 and before end period.";

    private const string InvalidDateRangeMessage =
        "Start date must be before or equal to end date.";

    private const string InvalidCapacityMessage =
        "Capacity must be greater than 0.";

    public async Task<CourseSectionResponse> CreateAsync(
        CreateCourseSectionRequest request,
        CancellationToken cancellationToken)
    {
        ValidateScheduleAndCapacity(
            request.StartPeriod,
            request.EndPeriod,
            request.StartDate,
            request.EndDate,
            request.Capacity);

        var normalizedSectionCode = request.SectionCode?.Trim() ?? string.Empty;
        if (normalizedSectionCode.Length is < 1 or > 50)
        {
            throw new BadRequestException("Section code must contain between 1 and 50 characters.");
        }

        if (request.TeacherUserRoleId <= 0)
        {
            throw new BadRequestException("Teacher user role ID must be a positive number.");
        }
        else{
            var teacher = await userRepository.GetByIdAsync(request.TeacherUserRoleId, cancellationToken);
            if (teacher is null || !teacher.UserRoles.Any(ur => !ur.IsDeleted && ur.Role.Code == SystemRoles.Teacher.Code))
            {
                throw new NotFoundException("The specified teacher could not be found.");
            }
        }
        if (request.SemesterId <= 0)
        {
            throw new BadRequestException("Semester ID must be a positive number.");
        }
        else{
            var semester = await semesterRepository.GetByIdAsync(request.SemesterId, cancellationToken);
            if (semester is null)
            {
                throw new NotFoundException("The specified semester could not be found.");
            }
        }
        if (request.CourseId <= 0)
        {
            throw new BadRequestException("Course ID must be a positive number.");
        }
        else{
            var course = await courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
            if (course is null)
            {
                throw new NotFoundException("The specified course could not be found.");
            }
        }

        var existingSection = await courseSectionRepository.GetBySemesterAndCodeAsync(
            request.SemesterId,
            normalizedSectionCode,
            cancellationToken);

        if (existingSection is not null)
        {
            throw new ConflictException(DuplicateSectionCodeMessage);
        }

        var entity = new CourseSection
        {
            CourseId = request.CourseId,
            SemesterId = request.SemesterId,
            TeacherUserRoleId = request.TeacherUserRoleId,
            SectionCode = normalizedSectionCode,
            Capacity = request.Capacity,
            DayOfWeek = request.DayOfWeek,
            StartPeriod = request.StartPeriod,
            EndPeriod = request.EndPeriod,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            var created = await courseSectionRepository.AddAsync(entity, cancellationToken);
            var response = ToResponse(created);
            return response;
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

        var response = ToResponse(section);
        return response;
    }

    public async Task<(IReadOnlyList<CourseSectionResponse> Items, int TotalItems)> TeacherGetPagedAsync(
    long? teacherUserRoleId,
    CourseSectionQueryParameters parameters,
    CancellationToken cancellationToken)
    {
        if (!teacherUserRoleId.HasValue)
        {
            return (Array.Empty<CourseSectionResponse>(), 0);
        }

        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);

        var result = await courseSectionRepository.GetPagedForTeacherAsync(
            teacherUserRoleId.Value,
            parameters,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = result.Items.Select(ToResponse).ToArray();
        return (items, result.TotalItems);
    }

    public async Task<(IReadOnlyList<CourseSectionResponse> Items, int TotalItems)> StudentGetPagedAsync(
        long? studentUserRoleId,
        CourseSectionQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        if (!studentUserRoleId.HasValue)
        {
            return (Array.Empty<CourseSectionResponse>(), 0);
        }

        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);

        var result = await courseSectionRepository.GetPagedForStudentAsync(
            studentUserRoleId.Value,
            parameters,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = result.Items.Select(ToResponse).ToArray();
        return (items, result.TotalItems);
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
            request.StartPeriod,
            request.EndPeriod,
            request.StartDate,
            request.EndDate,
            request.Capacity);

        if (request.TeacherUserRoleId.HasValue)
        {
            var teacher = await userRepository.GetByIdAsync(request.TeacherUserRoleId.Value, cancellationToken);
            if (teacher is null || !teacher.UserRoles.Any(ur => !ur.IsDeleted && ur.Role.Code == SystemRoles.Teacher.Code))
            {
                throw new NotFoundException("The specified teacher could not be found.");
            }
            existing.TeacherUserRoleId = request.TeacherUserRoleId.Value;
        }
        if (request.SemesterId.HasValue)
        {
            var semester = await semesterRepository.GetByIdAsync(request.SemesterId.Value, cancellationToken);
            if (semester is null)
            {
                throw new NotFoundException("The specified semester could not be found.");
            }
            existing.SemesterId = request.SemesterId.Value;
        }

        existing.Capacity = request.Capacity;
        existing.DayOfWeek = request.DayOfWeek;
        existing.StartPeriod = request.StartPeriod;
        existing.EndPeriod = request.EndPeriod;
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

            var response = ToResponse(updated);
            return response;
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

    public async Task<CourseSectionDetailResponse?> TeacherGetCourseSectionDetailsAsync(
        long teacherUserRoleId,
        long courseSectionId,
        PaginationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);

        var result = await courseSectionRepository.GetCourseSectionDetailsWithStudentsAsync(
            teacherUserRoleId,
            courseSectionId,
            pageNumber,
            pageSize,
            cancellationToken);

        if (result.Section == null)
        {
            return null;
        }

        var totalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

        var pagedStudents = new PagedResponse<StudentInCourseSectionResponse>
        {
            Items = result.Students,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = result.TotalItems,
            TotalPages = totalPages
        };

        return new CourseSectionDetailResponse(
            Id: result.Section.Id,
            SectionCode: result.Section.SectionCode,
            CourseId: result.Section.CourseId,
            SemesterId: result.Section.SemesterId,
            Students: pagedStudents
        );
    }

    private static void ValidateScheduleAndCapacity(
        ClassPeriod startPeriod,
        ClassPeriod endPeriod,
        DateOnly startDate,
        DateOnly endDate,
        int capacity)
    {
        if (capacity <= 0)
        {
            throw new BadRequestException(InvalidCapacityMessage);
        }

        if ((int)startPeriod < 1 || (int)startPeriod > 10 ||
            (int)endPeriod < 1 || (int)endPeriod > 10 ||
            startPeriod >= endPeriod)
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

    private static CourseSectionResponse ToResponse(CourseSection entity) => new()
    {
        Id = entity.Id,
        CourseId = entity.CourseId,
        SemesterId = entity.SemesterId,
        TeacherUserRoleId = entity.TeacherUserRoleId,
        SectionCode = entity.SectionCode,
        Capacity = entity.Capacity,
        DayOfWeek = entity.DayOfWeek,
        StartPeriod = entity.StartPeriod,
        EndPeriod = entity.EndPeriod,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = entity.Status,
        CreatedAt = AsUtc(entity.CreatedAt),
        UpdatedAt = AsUtc(entity.UpdatedAt)
    };

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}