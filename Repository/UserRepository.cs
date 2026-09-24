using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Maiven_Portal_Managment.Models.Enums;
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

    public async Task<(IReadOnlyList<User> Items, int TotalItems)> GetPagedAsync(
        string? name,
        string? email,
        UserRoleFilter? role,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            name = name.Trim();
            query = query.Where(user =>
            user.FullName.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            email = email.Trim();
            query = query.Where(user =>
            user.Email.Contains(email));
        }

        if (role.HasValue)
        {
            var roleValue = role.Value.ToString();
            query = query.Where(user =>
            user.UserRoles.Any(userRole => userRole.Role.Code == roleValue));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderBy(user => user.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    return (users, totalItems);
    }

    public async Task DeleteByIdAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var utcNow = DateTime.UtcNow;

            await dbContext.FaceCredentials
                .Where(credential => credential.UserId == userId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(credential => credential.IsDeleted, true)
                        .SetProperty(credential => credential.Embedding, (byte[]?)null)
                        .SetProperty(credential => credential.DeletedAt, utcNow)
                        .SetProperty(credential => credential.UpdatedAt, utcNow),
                    cancellationToken);

            await dbContext.Users
                .Where(user => user.Id == userId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(user => user.IsDeleted, true)
                        .SetProperty(user => user.UpdatedAt, utcNow),
                    cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
