using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class RegistrationPeriodRepository(AppDbContext dbContext)
{
    public async Task<RegistrationPeriod> AddAsync(
        RegistrationPeriod entity,
        CancellationToken cancellationToken)
    {
        dbContext.RegistrationPeriods.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return entity;
    }

    public async Task<RegistrationPeriod?> GetTrackedByIdAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.RegistrationPeriods
            .SingleOrDefaultAsync(
                rp => rp.Id == id && !rp.IsDeleted,
                cancellationToken);

        return entity;
    }

    public async Task<RegistrationPeriod?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.RegistrationPeriods
            .AsNoTracking()
            .SingleOrDefaultAsync(
                rp => rp.Id == id && !rp.IsDeleted,
                cancellationToken);

        return entity;
    }

    public async Task<IReadOnlyList<RegistrationPeriod>> GetAllAsync(
        long? semesterId,
        RegistrationPeriodStatus? status,
        CancellationToken cancellationToken)
    {
        var query = dbContext.RegistrationPeriods
            .AsNoTracking()
            .Where(rp => !rp.IsDeleted);

        if (semesterId.HasValue)
        {
            query = query.Where(rp => rp.SemesterId == semesterId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(rp => rp.Status == status.Value);
        }

        var entities = await query
            .OrderBy(rp => rp.StartAt)
            .ToListAsync(cancellationToken);

        return entities;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
