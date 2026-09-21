using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Models.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class AnnouncementRepository(AppDbContext dbContext)
{
    public async Task<AnnouncementModel?> GetByIdAsync(long announcementId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Announcements
            .AsNoTracking()
            .SingleOrDefaultAsync(announcement => announcement.Id == announcementId, cancellationToken);
        return entity?.ToModel();
    }

    public async Task<(IReadOnlyList<AnnouncementModel> Items, int TotalItems)> GetPagedAsync(
        long? sectionId,
        string? title,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Announcements.AsNoTracking();

        if (sectionId.HasValue)
        {
            query = query.Where(announcement => announcement.SectionId == sectionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(announcement => announcement.Title.Contains(title));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderByDescending(announcement => announcement.CreatedAt)
            .ThenByDescending(announcement => announcement.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (entities.Select(announcement => announcement.ToModel()).ToArray(), totalItems);
    }

    public async Task<AnnouncementModel> AddAsync(AnnouncementModel model, CancellationToken cancellationToken)
    {
        var entity = model.ToNewEntity();
        dbContext.Announcements.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToModel();
    }

    public async Task<AnnouncementModel?> UpdateAsync(AnnouncementModel model, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Announcements
            .SingleOrDefaultAsync(announcement => announcement.Id == model.Id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        model.ApplyToEntity(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToModel();
    }

    public async Task<bool> DeleteAsync(long announcementId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Announcements
            .SingleOrDefaultAsync(announcement => announcement.Id == announcementId, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.IsDeleted = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}