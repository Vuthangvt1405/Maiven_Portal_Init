using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Models.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class SemesterRepository(AppDbContext dbContext)
{
    public async Task<IReadOnlyList<SemesterModel>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var semesters = await dbContext.Semesters
            .AsNoTracking()
            .OrderByDescending(semester => semester.StartDate)
            .ThenByDescending(semester => semester.Id)
            .ToListAsync(cancellationToken);

        return semesters.Select(semester => semester.ToModel()).ToArray();
    }

    public async Task<SemesterModel?> GetByIdAsync(
        long semesterId,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Semesters
            .AsNoTracking()
            .SingleOrDefaultAsync(
                semester => semester.Id == semesterId,
                cancellationToken);

        return entity?.ToModel();
    }

    public async Task<IReadOnlyList<SemesterModel>> GetByAcademicYearIdAsync(
        long academicYearId,
        CancellationToken cancellationToken)
    {
        var semesters = await dbContext.Semesters
            .AsNoTracking()
            .Where(semester => semester.AcademicYearId == academicYearId)
            .ToListAsync(cancellationToken);

        return semesters.Select(semester => semester.ToModel()).ToArray();
    }

    public async Task<SemesterModel> CreateAsync(
        SemesterModel model,
        CancellationToken cancellationToken)
    {
        var entity = model.ToNewEntity();
        dbContext.Semesters.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToModel();
    }

    public async Task<SemesterModel?> UpdateAsync(
        SemesterModel model,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.Semesters.SingleOrDefaultAsync(
            semester => semester.Id == model.Id,
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
