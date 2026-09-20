using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Models.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class AcademicYearRepository(AppDbContext dbContext)
{
    public async Task<IReadOnlyList<AcademicYearModel>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var entities = await dbContext.AcademicYears
            .AsNoTracking()
            .OrderByDescending(academicYear => academicYear.StartDate)
            .ThenByDescending(academicYear => academicYear.Id)
            .ToListAsync(cancellationToken);

        return entities.Select(academicYear => academicYear.ToModel()).ToArray();
    }

    public async Task<AcademicYearModel?> GetByIdAsync(
        long academicYearId,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.AcademicYears
            .AsNoTracking()
            .SingleOrDefaultAsync(
                academicYear => academicYear.Id == academicYearId,
                cancellationToken);

        return entity?.ToModel();
    }

    public async Task<IReadOnlyList<SemesterModel>> GetSemestersAsync(
        long academicYearId,
        CancellationToken cancellationToken)
    {
        var semesters = await dbContext.Semesters
            .AsNoTracking()
            .Where(semester => semester.AcademicYearId == academicYearId)
            .ToListAsync(cancellationToken);

        return semesters.Select(semester => semester.ToModel()).ToArray();
    }

    public async Task<AcademicYearModel> AddAsync(
        AcademicYearModel model,
        CancellationToken cancellationToken)
    {
        var entity = model.ToNewEntity();
        dbContext.AcademicYears.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToModel();
    }

    public async Task<AcademicYearModel?> UpdateAsync(
        AcademicYearModel model,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.AcademicYears.SingleOrDefaultAsync(
            academicYear => academicYear.Id == model.Id,
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
