using Maiven_Portal_Managment.Dtos;
using Maiven_Portal_Managment.Models;

namespace Maiven_Portal_Managment.Repository.Interfaces;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<AuthAccount?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<AuthAccount> CreateStudentAsync(
        UserModel user,
        string passwordHash,
        CancellationToken cancellationToken);
    Task UpdatePasswordHashAsync(
        long userId,
        string passwordHash,
        CancellationToken cancellationToken);
}
