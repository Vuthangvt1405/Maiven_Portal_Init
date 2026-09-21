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

        return user.ToModel();
    }
}