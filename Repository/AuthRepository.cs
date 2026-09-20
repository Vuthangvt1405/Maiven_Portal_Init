using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Data.Entities.Enums;
using Maiven_Portal_Managment.Dtos;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Models.Mappings;
using Maiven_Portal_Managment.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class AuthRepository(AppDbContext dbContext) : IAuthRepository
{
    private const string StudentRoleCode = "STUDENT";

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
                RoleCodes = user.UserRoles
                    .Where(userRole => userRole.Status == ActiveStatus.ACTIVE)
                    .Where(userRole => userRole.Role.Status == ActiveStatus.ACTIVE)
                    .Select(userRole => userRole.Role.Code)
                    .Distinct()
                    .ToArray()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return null;
        }

        return new AuthAccount
        {
            User = result.User.ToModel(),
            PasswordHash = result.User.PasswordHash,
            RoleCodes = result.RoleCodes
        };
    }

    public async Task<AuthAccount> CreateStudentAsync(
        UserModel user,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        var studentRole = await dbContext.Roles
            .SingleOrDefaultAsync(
                role => role.Code == StudentRoleCode && role.Status == ActiveStatus.ACTIVE,
                cancellationToken)
            ?? throw new InvalidOperationException(
                "The required active STUDENT role is not configured.");

        var entity = user.ToNewEntity(passwordHash);
        entity.UserRoles.Add(new UserRole
        {
            RoleId = studentRole.Id,
            Status = ActiveStatus.ACTIVE
        });

        dbContext.Users.Add(entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            throw new ConflictException("An account with this email already exists.", exception);
        }

        return new AuthAccount
        {
            User = entity.ToModel(),
            PasswordHash = entity.PasswordHash,
            RoleCodes = [studentRole.Code]
        };
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

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.GetBaseException() is SqlException { Number: 2601 or 2627 };
}
