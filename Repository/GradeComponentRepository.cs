using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class GradeComponentRepository(AppDbContext dbContext)
{
    public async Task<IReadOnlyList<GradeComponent>> UpdateWeightsAsync(
        long sectionId,
        IReadOnlyDictionary<long, decimal> weights,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var components = await dbContext.GradeComponents
            .Where(component => component.SectionId == sectionId)
            .OrderBy(component => component.Id)
            .ToListAsync(cancellationToken);

        if (components.Count != weights.Count ||
            components.Any(component => !weights.ContainsKey(component.Id)))
        {
            throw new InvalidOperationException("The grade component set changed while updating weights.");
        }

        foreach (var component in components)
        {
            component.Weight = weights[component.Id];
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return components;
    }
}