using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class AuthRepository(AppDbContext dbContext)
{
    public Task<bool> EmailExistsAsync(
        string normalizedEmail,
        CancellationToken cancellationToken) =>
        dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(user => user.Email == normalizedEmail, cancellationToken);

    public async Task<AuthAccount?> FindByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        var result = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Email == normalizedEmail)
            .Select(user => new
            {
                User = user,
                RoleAssignments = user.UserRoles
                    .Select(userRole => new AuthRoleAssignment
                    {
                        RoleUserId = userRole.Id,
                        RoleCode = userRole.Role.Code
                    })
                    .ToArray()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return null;
        }

        return new AuthAccount
        {
            User = result.User,
            PasswordHash = result.User.PasswordHash,
            RoleAssignments = result.RoleAssignments
        };
    }

    public async Task<AuthAccount?> FindByUserIdAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var result = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new
            {
                User = user,
                RoleAssignments = user.UserRoles
                    .Select(userRole => new AuthRoleAssignment
                    {
                        RoleUserId = userRole.Id,
                        RoleCode = userRole.Role.Code
                    })
                    .ToArray()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return null;
        }

        return new AuthAccount
        {
            User = result.User,
            PasswordHash = result.User.PasswordHash,
            RoleAssignments = result.RoleAssignments
        };
    }

    public async Task<Role?> GetRoleByCodeAsync(
        string roleCode,
        CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .SingleOrDefaultAsync(
                role => role.Code == roleCode,
                cancellationToken);

        return role;
    }

    public async Task<User> AddUserAsync(
        User user,
        CancellationToken cancellationToken)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdatePasswordHashAsync(
        long userId,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        await dbContext.Users
            .Where(user => user.Id == userId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(user => user.PasswordHash, passwordHash)
                    .SetProperty(user => user.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
    }

    public static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.GetBaseException() is SqlException { Number: 2601 or 2627 };
}
