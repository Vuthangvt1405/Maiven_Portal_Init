using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities.Enums;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Models.Mappings;
using Maiven_Portal_Managment.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private const string StudentRoleCode = "STUDENT";

    public async Task<UserModel?> UpdateStudentProfileAsync(
        long userId,
        UserModel profile,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Where(user => user.Id == userId)
            .Where(user =>
                user.UserRoles.Any(userRole =>
                    userRole.Status == ActiveStatus.ACTIVE &&
                    userRole.Role.Status == ActiveStatus.ACTIVE &&
                    userRole.Role.Code == StudentRoleCode))
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.FullName = profile.FullName;
        user.DateOfBirth = profile.DateOfBirth;
        user.Gender = profile.Gender;
        user.Phone = profile.Phone;
        user.Address = profile.Address;
        user.AvatarUrl = profile.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return user.ToModel();
    }
}