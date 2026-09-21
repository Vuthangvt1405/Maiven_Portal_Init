using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Models.Mappings;
using Microsoft.EntityFrameworkCore;
using EntityGender = Maiven_Portal_Managment.Data.Entities.Enums.Gender;

namespace Maiven_Portal_Managment.Repository;

public sealed class UserRepository(AppDbContext dbContext)
{
    public async Task<UserModel?> UpdateProfileAsync(
        long userId,
        UserModel profile,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

    

        user.FullName = profile.FullName;
        user.DateOfBirth = profile.DateOfBirth;
        user.Gender = profile.Gender is not null ? (EntityGender)profile.Gender.Value : null;
        user.Phone = profile.Phone;
        user.Address = profile.Address;
        user.AvatarUrl = profile.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToModelWithRole(user);
    }
    public async Task<IReadOnlyList<UserModel>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .OrderBy(user => user.Id)
            .ToListAsync(cancellationToken);

        return users.Select(ToModelWithRole).ToArray();
    }

    public async Task<UserModel?> GetByIdAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

        return user is null ? null : ToModelWithRole(user);
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

    private static UserModel ToModelWithRole(Data.Entities.User user)
    {
        var model = user.ToModel();
        var userRole = user.UserRoles.FirstOrDefault();

        model.Role = userRole?.Role.Code ?? string.Empty;
        model.RoleUserId = userRole?.Id ?? 0;
        return model;
    }
}