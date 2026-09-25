using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class PasswordResetRepository(AppDbContext dbContext)
{
    public Task<User?> FindUserByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);

    public async Task CreateAsync(PasswordResetRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        await dbContext.PasswordResetRequests
            .Where(item => item.UserId == request.UserId && item.ConsumedAtUtc == null && item.InvalidatedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.InvalidatedAtUtc, now)
                .SetProperty(item => item.UpdatedAt, now), cancellationToken);
        dbContext.PasswordResetRequests.Add(request);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<PasswordResetRequest?> GetLatestOtpAsync(long userId, CancellationToken cancellationToken) =>
        dbContext.PasswordResetRequests
            .Where(item => item.UserId == userId && item.Method == "OTP" && item.ConsumedAtUtc == null && item.InvalidatedAtUtc == null)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<PasswordResetRequest?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        dbContext.PasswordResetRequests
            .Include(item => item.User)
            .SingleOrDefaultAsync(item => item.ResetTokenHash == tokenHash, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
