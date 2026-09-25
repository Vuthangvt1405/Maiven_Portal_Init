using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class FaceCredentialRepository(AppDbContext dbContext)
{
    public Task<FaceCredential?> GetActiveByUserIdAsync(
        long userId,
        CancellationToken cancellationToken) =>
        dbContext.FaceCredentials
            .AsNoTracking()
            .SingleOrDefaultAsync(
                credential => credential.UserId == userId,
                cancellationToken);

    public async Task<IReadOnlyList<FaceCredential>> GetActiveForModelAsync(
        string modelName,
        string modelVersion,
        int embeddingDimension,
        CancellationToken cancellationToken)
    {
        return await dbContext.FaceCredentials
            .AsNoTracking()
            .Where(credential =>
                credential.ModelName == modelName &&
                credential.ModelVersion == modelVersion &&
                credential.EmbeddingDimension == embeddingDimension &&
                credential.Embedding != null)
            .OrderBy(credential => credential.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FaceCredential>> GetActiveForModelExceptUserAsync(
        string modelName,
        string modelVersion,
        int embeddingDimension,
        long excludedUserId,
        CancellationToken cancellationToken)
    {
        return await dbContext.FaceCredentials
            .AsNoTracking()
            .Where(credential =>
                credential.UserId != excludedUserId &&
                credential.ModelName == modelName &&
                credential.ModelVersion == modelVersion &&
                credential.EmbeddingDimension == embeddingDimension &&
                credential.Embedding != null)
            .OrderBy(credential => credential.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceAsync(
        FaceCredential newCredential,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(newCredential);

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var utcNow = DateTime.UtcNow;

            await dbContext.FaceCredentials
                .Where(credential => credential.UserId == newCredential.UserId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(credential => credential.IsDeleted, true)
                        .SetProperty(credential => credential.Embedding, (byte[]?)null)
                        .SetProperty(credential => credential.DeletedAt, utcNow)
                        .SetProperty(credential => credential.UpdatedAt, utcNow),
                    cancellationToken);

            dbContext.FaceCredentials.Add(newCredential);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            dbContext.Entry(newCredential).State = EntityState.Detached;
            throw;
        }
    }

    public async Task<bool> SoftDeleteByUserIdAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var affectedRows = await dbContext.FaceCredentials
            .Where(credential => credential.UserId == userId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(credential => credential.IsDeleted, true)
                    .SetProperty(credential => credential.Embedding, (byte[]?)null)
                    .SetProperty(credential => credential.DeletedAt, utcNow)
                    .SetProperty(credential => credential.UpdatedAt, utcNow),
                cancellationToken);

        return affectedRows > 0;
    }
}
