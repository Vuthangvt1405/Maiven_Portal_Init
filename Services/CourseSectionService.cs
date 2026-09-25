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

    private const string OutsideSemesterMessage =
        "The course section date range must be within the selected semester.";

    public async Task<CourseSectionResponse> CreateAsync(
        CreateCourseSectionRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureTeacherExistsAsync(request.TeacherUserRoleId, cancellationToken);
        var semester = await EnsureSemesterExistsAsync(request.SemesterId, cancellationToken);
        var course = await EnsureCourseExistsAsync(request.CourseId, cancellationToken);
        EnsureSectionDatesWithinSemester(semester, request.StartDate, request.EndDate);
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
        created.Course = course;
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

        Semester semester;
        if (existing.SemesterId != request.SemesterId)
        {
            semester = await EnsureSemesterExistsAsync(request.SemesterId, cancellationToken);
            existing.SemesterId = request.SemesterId;
        }
        else
        {
            semester = await semesterRepository.GetByIdAsync(existing.SemesterId, cancellationToken)
                ?? throw new NotFoundException("The specified semester could not be found.");
        }

        EnsureSectionDatesWithinSemester(semester, request.StartDate, request.EndDate);

        existing.Capacity = request.Capacity;
        existing.DayOfWeek = request.DayOfWeek;
        existing.StartPeriod = (ClassPeriod)request.StartPeriod;
        existing.EndPeriod = (ClassPeriod)request.EndPeriod;
        existing.StartDate = request.StartDate;
        existing.EndDate = request.EndDate;
        existing.Status = request.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        await courseSectionRepository.SaveChangesAsync(cancellationToken);

        if (existing.Course is null)
        {
            existing.Course = await courseRepository.GetByIdAsync(existing.CourseId, cancellationToken)
                ?? throw new NotFoundException("The specified course could not be found.");
        }

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
            parameters.CourseName,
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
                ToCourseResponse(x.Section.Course),
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

    public async Task<(IReadOnlyList<CourseSectionWithEnrollmentCount> Items, int TotalItems)> TeacherGetPagedAsync(
    long teacherUserRoleId,
    CourseSectionQueryParameters parameters,
    CancellationToken cancellationToken)
    {
        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);

        var result = await courseSectionRepository.GetPagedForTeacherAsync(
            teacherUserRoleId,
            parameters.CourseId,
            parameters.CourseName,
            parameters.SemesterId,
            parameters.SectionCode,
            parameters.DayOfWeek,
            parameters.Status,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = result.Items.Select(ToWithEnrollmentCountResponse).ToArray();
        return (items, result.TotalItems);
    }

    public async Task<(IReadOnlyList<CourseSectionWithEnrollmentCount> Items, int TotalItems)> StudentGetPagedAsync(
        long studentUserRoleId,
        CourseSectionQueryParameters parameters,
        CancellationToken cancellationToken)
    {


        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);

        var result = await courseSectionRepository.GetPagedForStudentAsync(
            studentUserRoleId,
            parameters.CourseId,
            parameters.CourseName,
            parameters.SemesterId,
            parameters.SectionCode,
            parameters.DayOfWeek,
            parameters.Status,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = result.Items.Select(ToWithEnrollmentCountResponse).ToArray();
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

    public async Task<StudentGpaResponse> StudentGetGpaAsync(
        long studentUserRoleId,
        StudentGpaQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var enrollments = await courseSectionRepository.GetEnrollmentsForStudentGpaAsync(
            studentUserRoleId,
            cancellationToken);

        var academicYears = enrollments
            .GroupBy(enrollment => enrollment.Section.Semester.AcademicYearId)
            .OrderBy(group => group.First().Section.Semester.AcademicYear.StartDate)
            .ToArray();

        decimal cumulativeWeightedGradePoints = 0m;
        var cumulativeCompletedCredits = 0;
        var responses = new List<AcademicYearGpaResponse>();

        foreach (var academicYear in academicYears)
        {
            var semesters = academicYear
                .GroupBy(enrollment => enrollment.Section.SemesterId)
                .OrderBy(group => group.First().Section.Semester.StartDate)
                .Select(semester => BuildSemesterGpa(semester.ToArray()))
                .ToArray();

            foreach (var enrollment in academicYear)
            {
                if (!TryGetCompletedCourse(enrollment, out var gradePoint, out var credits))
                {
                    continue;
                }

                cumulativeWeightedGradePoints += gradePoint * credits;
                cumulativeCompletedCredits += credits;
            }

            var year = academicYear.First().Section.Semester.AcademicYear;
            responses.Add(new AcademicYearGpaResponse
            {
                AcademicYearId = year.Id,
                AcademicYearName = year.Name,
                CumulativeGpa = CalculateGpa(cumulativeWeightedGradePoints, cumulativeCompletedCredits),
                CumulativeCompletedCredits = cumulativeCompletedCredits,
                Semesters = semesters
            });
        }

        if (parameters.AcademicYearId.HasValue)
        {
            responses = responses
                .Where(response => response.AcademicYearId == parameters.AcademicYearId.Value)
                .ToList();
        }

        return new StudentGpaResponse { AcademicYears = responses };
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
            Course: ToCourseResponse(result.Section.Course),
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
                    score.Id,
                    score.ComponentId,
                    score.Component.Name,
                    score.Component.Weight,
                    score.Score))
                .ToList(),
            enrollment.CourseResult is null || enrollment.CourseResult.IsDeleted
                ? null
                : new StudentFinalResultResponse
                {
                    FinalScore = enrollment.CourseResult.FinalScore,
                    LetterGrade = enrollment.CourseResult.LetterGrade,
                    GradePoint = enrollment.CourseResult.GradePoint,
                    ResultStatus = enrollment.CourseResult.ResultStatus
                });

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

    private static SemesterGpaResponse BuildSemesterGpa(IReadOnlyList<Enrollment> enrollments)
    {
        decimal weightedGradePoints = 0m;
        var completedCredits = 0;

        foreach (var enrollment in enrollments)
        {
            if (!TryGetCompletedCourse(enrollment, out var gradePoint, out var credits))
            {
                continue;
            }

            weightedGradePoints += gradePoint * credits;
            completedCredits += credits;
        }

        var semester = enrollments[0].Section.Semester;
        return new SemesterGpaResponse
        {
            SemesterId = semester.Id,
            SemesterName = semester.Name,
            Gpa = CalculateGpa(weightedGradePoints, completedCredits),
            CompletedCredits = completedCredits
        };
    }

    private static bool TryGetCompletedCourse(Enrollment enrollment, out decimal gradePoint, out int credits)
    {
        gradePoint = 0m;
        credits = 0;

        if (enrollment.CourseResult is null ||
            enrollment.CourseResult.IsDeleted ||
            enrollment.CourseResult.GradePoint is not decimal resultGradePoint ||
            enrollment.Section.Course.Credits <= 0)
        {
            return false;
        }

        gradePoint = resultGradePoint;
        credits = enrollment.Section.Course.Credits;
        return true;
    }

    private static decimal? CalculateGpa(decimal weightedGradePoints, int completedCredits) =>
        completedCredits == 0
            ? null
            : decimal.Round(weightedGradePoints / completedCredits, 2, MidpointRounding.AwayFromZero);



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
    ToCourseResponse(entity.Course),
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

    private static CourseSectionWithEnrollmentCount ToWithEnrollmentCountResponse(
        (CourseSection Section, int EnrollmentCount) item) => new(
        item.Section.Id,
        ToCourseResponse(item.Section.Course),
        item.Section.SemesterId,
        item.Section.TeacherUserRoleId,
        item.Section.SectionCode,
        item.Section.Capacity,
        item.Section.DayOfWeek,
        (int)item.Section.StartPeriod,
        (int)item.Section.EndPeriod,
        item.EnrollmentCount,
        item.Section.StartDate,
        item.Section.EndDate,
        item.Section.Status,
        AsUtc(item.Section.CreatedAt),
        AsUtc(item.Section.UpdatedAt)
    );

    private static CourseResponse ToCourseResponse(Course course) => new(
        Id: course.Id,
        CourseCode: course.CourseCode,
        CourseName: course.CourseName,
        Credits: course.Credits,
        Description: course.Description,
        Status: course.Status,
        CreatedAt: AsUtc(course.CreatedAt),
        UpdatedAt: AsUtc(course.UpdatedAt)
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

    private async Task<Semester> EnsureSemesterExistsAsync(long semesterId, CancellationToken cancellationToken)
    {
        var semester = await semesterRepository.GetByIdAsync(semesterId, cancellationToken);
        if (semester is null)
        {
            throw new NotFoundException("The specified semester could not be found.");
        }

        return semester;
    }

    private async Task<Course> EnsureCourseExistsAsync(long courseId, CancellationToken cancellationToken)
    {
        var course = await courseRepository.GetByIdAsync(courseId, cancellationToken);
        if (course is null)
        {
            throw new NotFoundException("The specified course could not be found.");
        }

        return course;
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

    private static void EnsureSectionDatesWithinSemester(Semester semester, DateOnly startDate, DateOnly endDate)
    {
        if (startDate < semester.StartDate || endDate > semester.EndDate)
        {
            throw new BadRequestException(OutsideSemesterMessage);
        }
    }
}
