using Maiven_Portal_Managment.Models;

namespace Maiven_Portal_Managment.Repository.Interfaces;

public interface IUserRepository
{
    Task<UserModel?> UpdateStudentProfileAsync(
        long userId,
        UserModel profile,
        CancellationToken cancellationToken);
}