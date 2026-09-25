using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Data.Entities.Enums;
using Maiven_Portal_Managment.Dtos.request;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class EnrollmentRepository(AppDbContext dbContext)
{
    public enum CreateResult
    {
        Success,
        Full,
        Conflict
    }

    public Task<Enrollment?> GetByIdAsync(long id, CancellationToken cancellationToken) =>
        dbContext.Enrollments
            .Include(x => x.StudentUserRole).ThenInclude(x => x.User)
            .Include(x => x.Section).ThenInclude(x => x.Course)
            .Include(x => x.Section).ThenInclude(x => x.Semester).ThenInclude(x => x.AcademicYear)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Enrollment> Items, int TotalItems)> GetPagedAsync(
        EnrollmentQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = dbContext.Enrollments
            .AsNoTracking()
            .Include(x => x.StudentUserRole).ThenInclude(x => x.User)
            .Include(x => x.Section).ThenInclude(x => x.Course)
            .Include(x => x.Section).ThenInclude(x => x.Semester).ThenInclude(x => x.AcademicYear)
            .AsQueryable();

        if (parameters.UserId.HasValue)
            query = query.Where(x => x.StudentUserRole.UserId == parameters.UserId.Value);
        if (parameters.SemesterId.HasValue)
            query = query.Where(x => x.Section.SemesterId == parameters.SemesterId.Value);
        if (parameters.AcademicYearId.HasValue)
            query = query.Where(x => x.Section.Semester.AcademicYearId == parameters.AcademicYearId.Value);
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            query = query.Where(x => x.StudentUserRole.User.FullName.Contains(search) ||
                                     x.StudentUserRole.User.Email.Contains(search) ||
                                     x.Section.SectionCode.Contains(search) ||
                                     x.Section.Course.CourseCode.Contains(search) ||
                                     x.Section.Course.CourseName.Contains(search));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var page = parameters.Page < 1 ? 1 : parameters.Page;
        var pageSize = parameters.PageSize < 1 ? 10 : Math.Min(parameters.PageSize, 100);
        var items = await query.OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public Task<bool> ExistsAsync(long userId, long sectionId, long? excludedId, CancellationToken cancellationToken) =>
        dbContext.Enrollments.AnyAsync(x => x.StudentUserRole.UserId == userId &&
                                            x.SectionId == sectionId &&
                                            (!excludedId.HasValue || x.Id != excludedId.Value), cancellationToken);

    public Task<int> CountInSectionAsync(long sectionId, long? excludedId, CancellationToken cancellationToken) =>
        dbContext.Enrollments.CountAsync(x => x.SectionId == sectionId &&
                                             (!excludedId.HasValue || x.Id != excludedId.Value), cancellationToken);

    public Task<UserRole?> GetStudentRoleAsync(long userId, CancellationToken cancellationToken) =>
        dbContext.UserRoles.Include(x => x.Role).Include(x => x.User)
            .SingleOrDefaultAsync(x => x.UserId == userId && x.Role.Code == "STUDENT", cancellationToken);

    public Task<CourseSection?> GetSectionAsync(long sectionId, CancellationToken cancellationToken) =>
        dbContext.CourseSections.Include(x => x.Course).Include(x => x.Semester)
            .SingleOrDefaultAsync(x => x.Id == sectionId, cancellationToken);

    public async Task<CreateResult> TryCreateAsync(
        Enrollment entity,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var section = await dbContext.CourseSections
                .FromSqlInterpolated($"SELECT * FROM [COURSE_SECTIONS] WITH (UPDLOCK, HOLDLOCK) WHERE [id] = {entity.SectionId} AND [isDelete] = 0")
                .SingleOrDefaultAsync(cancellationToken);

            if (section is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return CreateResult.Conflict;
            }

            var deletedEnrollment = await dbContext.Enrollments
                .IgnoreQueryFilters()
                .Include(x => x.StudentScores)
                .Include(x => x.CourseResult)
                .SingleOrDefaultAsync(x => x.StudentUserRoleId == entity.StudentUserRoleId &&
                                           x.SectionId == entity.SectionId &&
                                           x.IsDeleted,
                    cancellationToken);

            var currentCapacity = await dbContext.Enrollments
                .CountAsync(x => x.SectionId == entity.SectionId && !x.IsDeleted, cancellationToken);

            if (currentCapacity >= section.Capacity && deletedEnrollment is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return CreateResult.Full;
            }

            var duplicate = await dbContext.Enrollments.AnyAsync(
                x => x.StudentUserRoleId == entity.StudentUserRoleId &&
                     x.SectionId == entity.SectionId &&
                     !x.IsDeleted,
                cancellationToken);

            if (duplicate)
            {
                await transaction.RollbackAsync(cancellationToken);
                return CreateResult.Conflict;
            }

            var hasScheduleOverlap = await dbContext.Enrollments
                .AnyAsync(enrollment =>
                    enrollment.StudentUserRoleId == entity.StudentUserRoleId &&
                    !enrollment.IsDeleted &&
                    enrollment.SectionId != entity.SectionId &&
                    enrollment.Section.SemesterId == section.SemesterId &&
                    enrollment.Section.DayOfWeek == section.DayOfWeek &&
                    section.StartPeriod < enrollment.Section.EndPeriod &&
                    section.EndPeriod > enrollment.Section.StartPeriod,
                    cancellationToken);

            if (hasScheduleOverlap)
            {
                await transaction.RollbackAsync(cancellationToken);
                return CreateResult.Conflict;
            }

            if (deletedEnrollment is not null)
            {
                deletedEnrollment.IsDeleted = false;

                foreach (var score in deletedEnrollment.StudentScores)
                {
                    score.IsDeleted = false;
                }

                if (deletedEnrollment.CourseResult is not null)
                {
                    deletedEnrollment.CourseResult.IsDeleted = false;
                }

                entity.Id = deletedEnrollment.Id;
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return CreateResult.Success;
            }

            dbContext.Enrollments.Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            var capacityAfterInsert = await dbContext.Enrollments
                .CountAsync(x => x.SectionId == entity.SectionId && !x.IsDeleted, cancellationToken);

            if (capacityAfterInsert > section.Capacity)
            {
                await transaction.RollbackAsync(cancellationToken);
                return CreateResult.Conflict;
            }

            var components = await dbContext.GradeComponents
                .Where(x => x.SectionId == entity.SectionId)
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);

            foreach (var component in components)
            {
                dbContext.StudentScores.Add(new StudentScore
                {
                    EnrollmentId = entity.Id,
                    ComponentId = component.Id,
                    Score = 0m,
                    UpdatedById = -1,
                    IsDeleted = false,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            dbContext.CourseResults.Add(new CourseResult
            {
                EnrollmentId = entity.Id,
                FinalScore = 0m,
                GradePoint = 0m,
                ResultStatus = ResultStatus.INCOMPLETE,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return CreateResult.Success;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<Enrollment> AddAsync(Enrollment entity, CancellationToken cancellationToken)
    {
        dbContext.Enrollments.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Enrollment?> UpdateAsync(Enrollment values, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Enrollments.SingleOrDefaultAsync(x => x.Id == values.Id, cancellationToken);
        if (entity is null) return null;
        entity.StudentUserRoleId = values.StudentUserRoleId;
        entity.SectionId = values.SectionId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(long studentUserRoleId, long sectionId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Enrollments
            .Include(x => x.StudentScores)
            .Include(x => x.CourseResult)
            .SingleOrDefaultAsync(x => x.StudentUserRoleId == studentUserRoleId &&
                                       x.SectionId == sectionId &&
                                       !x.IsDeleted,
                cancellationToken);
        if (entity is null) return false;

        foreach (var score in entity.StudentScores.Where(score => !score.IsDeleted))
        {
            dbContext.StudentScores.Remove(score);
        }

        if (entity.CourseResult is { IsDeleted: false } courseResult)
        {
            dbContext.CourseResults.Remove(courseResult);
        }

        dbContext.Enrollments.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}