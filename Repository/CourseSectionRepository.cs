using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Models.Enums;
using Maiven_Portal_Managment.Models.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class CourseSectionRepository(AppDbContext dbContext)
{
    public async Task<CourseSectionModel?> GetByIdAsync(
        long sectionId,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.CourseSections
            .AsNoTracking()
            .SingleOrDefaultAsync(
                s => s.Id == sectionId && !s.IsDeleted,
                cancellationToken);

        return entity?.ToModel();
    }

    public async Task<CourseSectionModel?> GetBySemesterAndCodeAsync(
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

        return entity?.ToModel();
    }

    public async Task<(IReadOnlyList<CourseSectionModel> Items, int TotalItems)> GetPagedAsync(
        long? courseId,
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
            .Where(s => !s.IsDeleted);

        if (courseId.HasValue)
        {
            query = query.Where(s => s.CourseId == courseId.Value);
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
            query = query.Where(s => (int)s.DayOfWeek == (int)dayOfWeek.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => (int)s.Status == (int)status.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var entities = await query
            .OrderBy(s => s.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = entities
            .Select(s => s.ToModel())
            .ToArray();

        return (items, totalItems);
    }

    public async Task<CourseSectionModel> AddAsync(
        CourseSectionModel model,
        CancellationToken cancellationToken)
    {
        var entity = model.ToNewEntity();

        dbContext.CourseSections.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }

    public async Task<CourseSectionModel?> UpdateAsync(
        CourseSectionModel model,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.CourseSections
            .SingleOrDefaultAsync(
                s => s.Id == model.Id && !s.IsDeleted,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        model.ApplyToEntity(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }
}