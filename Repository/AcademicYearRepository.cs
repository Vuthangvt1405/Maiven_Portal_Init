using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class AcademicYearRepository(AppDbContext dbContext)
{
    public async Task<IReadOnlyList<AcademicYear>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var entities = await dbContext.AcademicYears
            .AsNoTracking()
            .OrderByDescending(academicYear => academicYear.StartDate)
            .ThenByDescending(academicYear => academicYear.Id)
            .ToListAsync(cancellationToken);

        return entities;
    }

    public async Task<AcademicYear?> GetByIdAsync(
        long academicYearId,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.AcademicYears
            .AsNoTracking()
            .SingleOrDefaultAsync(
                academicYear => academicYear.Id == academicYearId,
                cancellationToken);

        return entity;
    }

    public async Task<IReadOnlyList<Semester>> GetSemestersAsync(
        long academicYearId,
        CancellationToken cancellationToken)
    {
        var semesters = await dbContext.Semesters
            .AsNoTracking()
            .Where(semester => semester.AcademicYearId == academicYearId)
            .ToListAsync(cancellationToken);

        return semesters;
    }

    public async Task<AcademicYear> AddAsync(
        AcademicYear entity,
        CancellationToken cancellationToken)
    {
        dbContext.AcademicYears.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<AcademicYear?> UpdateAsync(
        AcademicYear values,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.AcademicYears.SingleOrDefaultAsync(
            academicYear => academicYear.Id == values.Id,
            cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Name = values.Name;
        entity.StartDate = values.StartDate;
        entity.EndDate = values.EndDate;
        entity.Status = values.Status;

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }
}
