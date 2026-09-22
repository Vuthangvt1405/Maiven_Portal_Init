using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class SemesterRepository(AppDbContext dbContext)
{
    public async Task<IReadOnlyList<Semester>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var semesters = await dbContext.Semesters
            .AsNoTracking()
            .OrderByDescending(semester => semester.StartDate)
            .ThenByDescending(semester => semester.Id)
            .ToListAsync(cancellationToken);

        return semesters;
    }

    public async Task<Semester?> GetByIdAsync(
        long semesterId,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Semesters
            .AsNoTracking()
            .SingleOrDefaultAsync(
                semester => semester.Id == semesterId,
                cancellationToken);

        return entity;
    }

    public async Task<IReadOnlyList<Semester>> GetByAcademicYearIdAsync(
        long academicYearId,
        CancellationToken cancellationToken)
    {
        var semesters = await dbContext.Semesters
            .AsNoTracking()
            .Where(semester => semester.AcademicYearId == academicYearId)
            .ToListAsync(cancellationToken);

        return semesters;
    }

    public async Task<Semester> CreateAsync(
        Semester entity,
        CancellationToken cancellationToken)
    {
        dbContext.Semesters.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Semester?> UpdateAsync(
        Semester values,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Semesters.SingleOrDefaultAsync(
            semester => semester.Id == values.Id,
            cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.AcademicYearId = values.AcademicYearId;
        entity.Name = values.Name;
        entity.StartDate = values.StartDate;
        entity.EndDate = values.EndDate;

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }
}
