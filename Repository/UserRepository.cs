using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class UserRepository(AppDbContext dbContext)
{
    public async Task<User?> GetTrackedByIdAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

        return user;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .OrderBy(user => user.Id)
            .ToListAsync(cancellationToken);

        return users;
    }

    public async Task<User?> GetByIdAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

        return user;
    }

    public async Task DeleteByIdAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        await dbContext.Users
            .Where(user => user.Id == userId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(user => user.IsDeleted, true)
                    .SetProperty(user => user.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
    }
}
