using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Maiven_Portal_Managment.Models.Enums;
namespace Maiven_Portal_Managment.Repository;

public sealed class UserRoleRepository(AppDbContext dbContext)
{
    public async Task<UserRole?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var userRole = await dbContext.UserRoles
            .AsNoTracking()
            .Include(userRole => userRole.Role)
            .SingleOrDefaultAsync(userRole => userRole.Id == id, cancellationToken);

        return userRole;
    }



}
