using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class CourseSectionRepository(AppDbContext dbContext)
{
    public async Task<CourseSection?> GetByIdAsync(
        long sectionId,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.CourseSections
            .AsNoTracking()
            .Include(s => s.Course)
            .SingleOrDefaultAsync(
                s => s.Id == sectionId && !s.IsDeleted,
                cancellationToken);

        return entity;
    }

    public async Task<CourseSection?> GetTrackedByIdAsync(
        long sectionId,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.CourseSections
            .Include(s => s.Course)
            .SingleOrDefaultAsync(
                s => s.Id == sectionId && !s.IsDeleted,
                cancellationToken);

        return entity;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task<CourseSection?> GetBySemesterAndCodeAsync(
        long semesterId,
        string sectionCode,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.CourseSections
            .AsNoTracking()
            .SingleOrDefaultAsync(
                s => s.SemesterId == semesterId &&
                     s.SectionCode == sectionCode &&
                     !s.IsDeleted,
                cancellationToken);

        return entity;
    }

    public async Task<(IReadOnlyList<(CourseSection Section, int EnrollmentCount)> Items, int TotalItems)> GetPagedAsync(
    long? courseId,
    string? courseName,
    long? semesterId,
    long? teacherUserRoleId,
    string? sectionCode,
    WeekDay? dayOfWeek,
    CourseSectionStatus? status,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken)
    {
        var query = dbContext.CourseSections
            .AsNoTracking()
            .Include(s => s.Course)
            .Where(s => !s.IsDeleted);

        if (courseId.HasValue)
        {
            query = query.Where(s => s.CourseId == courseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(courseName))
        {
            var trimmedCourseName = courseName.Trim();
            query = query.Where(s => s.Course.CourseName.Contains(trimmedCourseName));
        }

        if (semesterId.HasValue)
        {
            query = query.Where(s => s.SemesterId == semesterId.Value);
        }

        if (teacherUserRoleId.HasValue)
        {
            query = query.Where(s => s.TeacherUserRoleId == teacherUserRoleId.Value);
        }

        if (!string.IsNullOrWhiteSpace(sectionCode))
        {
            query = query.Where(s => s.SectionCode.Contains(sectionCode));
        }

        if (dayOfWeek.HasValue)
        {
            query = query.Where(s => s.DayOfWeek == dayOfWeek.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var rawItems = await query
            .OrderBy(s => s.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                Section = s,
                EnrollmentCount = s.Enrollments.Count(e => !e.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        var items = rawItems
            .Select(x => (x.Section, x.EnrollmentCount))
            .ToList();

        return (items, totalItems);
    }

    public async Task<(IReadOnlyList<(CourseSection Section, int EnrollmentCount)> Items, int TotalItems)> GetPagedForTeacherAsync(
        long teacherUserRoleId,
        long? courseId,
        string? courseName,
        long? semesterId,
        string? sectionCode,
        WeekDay? dayOfWeek,
        CourseSectionStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.CourseSections
            .AsNoTracking()
            .Include(s => s.Course)
            .Where(s => !s.IsDeleted && s.TeacherUserRoleId == teacherUserRoleId);

        return await ApplyFiltersAndPagingAsync(
            query,
            courseId,
            courseName,
            semesterId,
            sectionCode,
            dayOfWeek,
            status,
            pageNumber,
            pageSize,
            cancellationToken);
    }

    public async Task<(IReadOnlyList<(CourseSection Section, int EnrollmentCount)> Items, int TotalItems)> GetPagedForStudentAsync(
        long studentUserRoleId,
        long? courseId,
        string? courseName,
        long? semesterId,
        string? sectionCode,
        WeekDay? dayOfWeek,
        CourseSectionStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.CourseSections
            .AsNoTracking()
            .Include(s => s.Course)
            .Where(s => !s.IsDeleted &&
                            s.Enrollments.Any(e => !e.IsDeleted
                                     && e.StudentUserRoleId == studentUserRoleId));

        return await ApplyFiltersAndPagingAsync(
            query,
            courseId,
            courseName,
            semesterId,
            sectionCode,
            dayOfWeek,
            status,
            pageNumber,
            pageSize,
            cancellationToken);
    }

    private static async Task<(IReadOnlyList<(CourseSection Section, int EnrollmentCount)> Items, int TotalItems)> ApplyFiltersAndPagingAsync(
        IQueryable<CourseSection> query,
        long? courseId,
        string? courseName,
        long? semesterId,
        string? sectionCode,
        WeekDay? dayOfWeek,
        CourseSectionStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (courseId.HasValue)
        {
            query = query.Where(s => s.CourseId == courseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(courseName))
        {
            var trimmedCourseName = courseName.Trim();
            query = query.Where(s => s.Course.CourseName.Contains(trimmedCourseName));
        }

        if (semesterId.HasValue)
        {
            query = query.Where(s => s.SemesterId == semesterId.Value);
        }

        if (!string.IsNullOrWhiteSpace(sectionCode))
        {
            query = query.Where(s => s.SectionCode.Contains(sectionCode));
        }

        if (dayOfWeek.HasValue)
        {
            query = query.Where(s => s.DayOfWeek == dayOfWeek.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var rawItems = await query
            .OrderBy(s => s.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                Section = s,
                EnrollmentCount = s.Enrollments.Count(e => !e.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        var items = rawItems
            .Select(x => (x.Section, x.EnrollmentCount))
            .ToList();

        return (items, totalItems);
    }

    public async Task<CourseSection> AddAsync(
        CourseSection entity,
        CancellationToken cancellationToken)
    {
        dbContext.CourseSections.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<(CourseSection? Section, IReadOnlyList<Enrollment> Enrollments, int TotalItems)> GetEnrollmentsForTeacherDetailAsync(
        long teacherUserRoleId,
        long sectionId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var sectionEntity = await dbContext.CourseSections
            .AsNoTracking()
            .Include(s => s.Course)
            .SingleOrDefaultAsync(
                s => s.Id == sectionId && s.TeacherUserRoleId == teacherUserRoleId && !s.IsDeleted,
                cancellationToken);

        if (sectionEntity == null)
        {
            return (null, Array.Empty<Enrollment>(), 0);
        }

        var enrollmentsQuery = dbContext.Enrollments
            .AsNoTracking()
            .Include(e => e.StudentUserRole)
                .ThenInclude(userRole => userRole.User)
            .Include(e => e.StudentScores)
                .ThenInclude(score => score.Component)
            .Include(e => e.CourseResult)
            .Where(e => e.SectionId == sectionId && !e.IsDeleted && !e.StudentUserRole.IsDeleted);

        var totalItems = await enrollmentsQuery.CountAsync(cancellationToken);

        var enrollments = await enrollmentsQuery
            .OrderBy(e => e.StudentUserRole.User.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (sectionEntity, enrollments, totalItems);
    }

    public async Task<(IReadOnlyList<Enrollment> Items, int TotalItems)> GetEnrollmentsWithResultsForStudentAsync(
        long studentUserRoleId,
        long? academicYearId,
        long? semesterId,
        long? courseId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Enrollments
            .AsNoTracking()
            .Include(e => e.Section)
                .ThenInclude(section => section.Course)
            .Include(e => e.Section)
                .ThenInclude(section => section.Semester)
                    .ThenInclude(semester => semester.AcademicYear)
            .Include(e => e.Section)
                .ThenInclude(section => section.GradeComponents)
            .Include(e => e.StudentScores)
                .ThenInclude(score => score.Component)
            .Include(e => e.CourseResult)
            .Where(e => !e.IsDeleted
                && e.StudentUserRoleId == studentUserRoleId
                && !e.StudentUserRole.IsDeleted
                && !e.Section.IsDeleted);

        if (academicYearId.HasValue)
        {
            query = query.Where(e => e.Section.Semester.AcademicYearId == academicYearId.Value);
        }

        if (semesterId.HasValue)
        {
            query = query.Where(e => e.Section.SemesterId == semesterId.Value);
        }

        if (courseId.HasValue)
        {
            query = query.Where(e => e.Section.CourseId == courseId.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .AsSplitQuery()
            .OrderBy(e => e.Section.Semester.AcademicYearId)
            .ThenBy(e => e.Section.SemesterId)
            .ThenBy(e => e.SectionId)
            .ThenBy(e => e.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

}
