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
    UserRoleRepository userRoleRepository,
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
        await EnsureTeacherExistsAsync(request.TeacherUserRoleId, cancellationToken);
        await EnsureSemesterExistsAsync(request.SemesterId, cancellationToken);
        await EnsureCourseExistsAsync(request.CourseId, cancellationToken);
        await EnsureSectionCodeNotDuplicatedAsync(request.SemesterId, request.SectionCode, cancellationToken);

        var entity = new CourseSection
        {
            CourseId = request.CourseId,
            SemesterId = request.SemesterId,
            TeacherUserRoleId = request.TeacherUserRoleId,
            SectionCode = request.SectionCode.Trim(),
            Capacity = request.Capacity,
            DayOfWeek = request.DayOfWeek,
            StartPeriod = (ClassPeriod)request.StartPeriod,
            EndPeriod = (ClassPeriod)request.EndPeriod,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var component in BuildDefaultGradeComponents())
        {
            entity.GradeComponents.Add(component);
        }

        var created = await courseSectionRepository.AddAsync(entity, cancellationToken);
        return ToResponse(created);
    }

    public async Task<CourseSectionResponse> UpdateAsync(
        long sectionId,
        UpdateCourseSectionRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await courseSectionRepository.GetTrackedByIdAsync(sectionId, cancellationToken);
        if (existing is null)
        {
            throw new NotFoundException("The course section could not be found.");
        }

        if (existing.TeacherUserRoleId != request.TeacherUserRoleId)
        {
            await EnsureTeacherExistsAsync(request.TeacherUserRoleId, cancellationToken);
            existing.TeacherUserRoleId = request.TeacherUserRoleId;
        }

        if (existing.SemesterId != request.SemesterId)
        {
            await EnsureSemesterExistsAsync(request.SemesterId, cancellationToken);
            existing.SemesterId = request.SemesterId;
        }

        existing.Capacity = request.Capacity;
        existing.DayOfWeek = request.DayOfWeek;
        existing.StartPeriod = (ClassPeriod)request.StartPeriod;
        existing.EndPeriod = (ClassPeriod)request.EndPeriod;
        existing.StartDate = request.StartDate;
        existing.EndDate = request.EndDate;
        existing.Status = request.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        await courseSectionRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(existing);
    }

    public async Task<(IReadOnlyList<CourseSectionWithEnrollmentCount> Items, int TotalItems)> GetPagedAsync(
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
            .Select(x => new CourseSectionWithEnrollmentCount(
                x.Section.Id,
                x.Section.CourseId,
                x.Section.SemesterId,
                x.Section.TeacherUserRoleId,
                x.Section.SectionCode,
                x.Section.Capacity,
                x.Section.DayOfWeek,
                (int)x.Section.StartPeriod,
                (int)x.Section.EndPeriod,
                x.EnrollmentCount,
                x.Section.StartDate,
                x.Section.EndDate,
                x.Section.Status,
                AsUtc(x.Section.CreatedAt),
                AsUtc(x.Section.UpdatedAt)
            ))
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
    long teacherUserRoleId,
    CourseSectionQueryParameters parameters,
    CancellationToken cancellationToken)
    {
        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);

        var result = await courseSectionRepository.GetPagedForTeacherAsync(
            teacherUserRoleId,
            parameters.CourseId,
            parameters.SemesterId,
            parameters.SectionCode,
            parameters.DayOfWeek,
            parameters.Status,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = result.Items.Select(ToResponse).ToArray();
        return (items, result.TotalItems);
    }

    public async Task<(IReadOnlyList<CourseSectionResponse> Items, int TotalItems)> StudentGetPagedAsync(
        long studentUserRoleId,
        CourseSectionQueryParameters parameters,
        CancellationToken cancellationToken)
    {


        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);

        var result = await courseSectionRepository.GetPagedForStudentAsync(
            studentUserRoleId,
            parameters.CourseId,
            parameters.SemesterId,
            parameters.SectionCode,
            parameters.DayOfWeek,
            parameters.Status,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = result.Items.Select(ToResponse).ToArray();
        return (items, result.TotalItems);
    }

    public async Task<(IReadOnlyList<StudentCourseResultResponse> Items, int TotalItems)> StudentGetResultsAsync(
        long studentUserRoleId,
        StudentCourseResultQueryParameters parameters,
        CancellationToken cancellationToken)
    {

        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);

        var result = await courseSectionRepository.GetEnrollmentsWithResultsForStudentAsync(
            studentUserRoleId,
            parameters.AcademicYearId,
            parameters.SemesterId,
            parameters.CourseId,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = result.Items
            .Select(ToStudentResultResponse)
            .ToArray();

        return (items, result.TotalItems);
    }



    public async Task<CourseSectionDetailResponse?> TeacherGetCourseSectionDetailsAsync(
        long teacherUserRoleId,
        long courseSectionId,
        PaginationQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);

        var result = await courseSectionRepository.GetEnrollmentsForTeacherDetailAsync(
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
            Items = result.Enrollments.Select(ToStudentInSectionResponse).ToArray(),
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

    private static IReadOnlyList<GradeComponent> BuildDefaultGradeComponents() =>
        Enum.GetValues<DefaultGradeComponent>()
            .Select(type => new GradeComponent
            {
                Name = type.GetDescription(),
                Weight = 25m
            })
            .ToList();

    private static StudentInCourseSectionResponse ToStudentInSectionResponse(Enrollment enrollment) =>
        new(
            enrollment.Id,
            enrollment.StudentUserRoleId,
            enrollment.StudentUserRole.User.FullName,
            enrollment.StudentUserRole.User.Email,
            enrollment.StudentScores
                .Where(score => !score.IsDeleted)
                .Select(score => new StudentScoreDetailResponse(
                    score.ComponentId,
                    score.Component.Name,
                    score.Component.Weight,
                    score.Score))
                .ToList());

    private static StudentCourseResultResponse ToStudentResultResponse(Enrollment enrollment) => new()
    {
        EnrollmentId = enrollment.Id,
        SectionId = enrollment.SectionId,
        SectionCode = enrollment.Section.SectionCode,
        CourseId = enrollment.Section.CourseId,
        CourseCode = enrollment.Section.Course.CourseCode,
        CourseName = enrollment.Section.Course.CourseName,
        Credits = enrollment.Section.Course.Credits,
        SemesterId = enrollment.Section.SemesterId,
        SemesterName = enrollment.Section.Semester.Name,
        AcademicYearId = enrollment.Section.Semester.AcademicYearId,
        AcademicYearName = enrollment.Section.Semester.AcademicYear.Name,
        ComponentScores = enrollment.Section.GradeComponents
            .Where(component => !component.IsDeleted)
            .OrderBy(component => component.Id)
            .Select(component => new StudentComponentScoreResponse
            {
                ComponentId = component.Id,
                ComponentName = component.Name,
                Weight = component.Weight,
                Score = enrollment.StudentScores
                    .Where(score => score.ComponentId == component.Id && !score.IsDeleted)
                    .Select(score => score.Score)
                    .FirstOrDefault()
            })
            .ToList(),
        FinalResult = enrollment.CourseResult is null || enrollment.CourseResult.IsDeleted
            ? null
            : new StudentFinalResultResponse
            {
                FinalScore = enrollment.CourseResult.FinalScore,
                LetterGrade = enrollment.CourseResult.LetterGrade,
                GradePoint = enrollment.CourseResult.GradePoint,
                ResultStatus = enrollment.CourseResult.ResultStatus
            }
    };



    // private static ConflictException? TranslateUniqueConstraintException(DbUpdateException exception)
    // {
    //     if (exception.GetBaseException() is not SqlException sqlException ||
    //         sqlException.Number is not (2601 or 2627))
    //     {
    //         return null;
    //     }

    //     if (sqlException.Message.Contains("UX_COURSE_SECTIONS", StringComparison.OrdinalIgnoreCase))
    //     {
    //         return new ConflictException(DuplicateSectionCodeMessage, exception);
    //     }

    //     return new ConflictException("The course section conflicts with an existing record.", exception);
    // }

    private static CourseSectionResponse ToResponse(CourseSection entity) => new(
    entity.Id,
    entity.CourseId,
    entity.SemesterId,
    entity.TeacherUserRoleId,
    entity.SectionCode,
    entity.Capacity,
    entity.DayOfWeek,
    (int)entity.StartPeriod,
    (int)entity.EndPeriod,
    entity.StartDate,
    entity.EndDate,
    entity.Status,
    AsUtc(entity.CreatedAt),
    AsUtc(entity.UpdatedAt)
);

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private async Task EnsureTeacherExistsAsync(long teacherUserRoleId, CancellationToken cancellationToken)
    {
        var teacher = await userRoleRepository.GetByIdAsync(teacherUserRoleId, cancellationToken);
        if (teacher is null || teacher.Role?.Code != SystemRoles.Teacher.Code)
        {
            throw new NotFoundException("The specified teacher could not be found.");
        }
    }

    private async Task EnsureSemesterExistsAsync(long semesterId, CancellationToken cancellationToken)
    {
        var semester = await semesterRepository.GetByIdAsync(semesterId, cancellationToken);
        if (semester is null)
        {
            throw new NotFoundException("The specified semester could not be found.");
        }
    }

    private async Task EnsureCourseExistsAsync(long courseId, CancellationToken cancellationToken)
    {
        var course = await courseRepository.GetByIdAsync(courseId, cancellationToken);
        if (course is null)
        {
            throw new NotFoundException("The specified course could not be found.");
        }
    }

    private async Task EnsureSectionCodeNotDuplicatedAsync(long semesterId, string sectionCode, CancellationToken cancellationToken)
    {
        var existingSection = await courseSectionRepository.GetBySemesterAndCodeAsync(
            semesterId,
            sectionCode.Trim(),
            cancellationToken);

        if (existingSection is not null)
        {
            throw new ConflictException(DuplicateSectionCodeMessage);
        }
    }
}