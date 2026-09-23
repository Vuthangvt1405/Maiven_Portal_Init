using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class AnnouncementRepository(AppDbContext dbContext)
{
    public async Task<Announcement?> GetByIdAsync(long announcementId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Announcements
            .AsNoTracking()
            .Include(announcement => announcement.CreatedBy)
                .ThenInclude(user => user.UserRoles)
                    .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(announcement => announcement.Id == announcementId, cancellationToken);
        return entity;
    }

    public async Task<(IReadOnlyList<Announcement> Items, int TotalItems)> GetPagedAsync(
        long? sectionId,
        string? title,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Announcements
            .AsNoTracking()
            .Include(announcement => announcement.CreatedBy)
                .ThenInclude(user => user.UserRoles)
                    .ThenInclude(userRole => userRole.Role)
            .AsQueryable();

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

        return (entities, totalItems);
    }

    public async Task<Announcement?> GetTrackedByIdAsync(long announcementId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Announcements
            .Include(announcement => announcement.CreatedBy)
                .ThenInclude(user => user.UserRoles)
                    .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(announcement => announcement.Id == announcementId, cancellationToken);
        return entity;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task<Announcement> AddAsync(Announcement entity, CancellationToken cancellationToken)
    {
        dbContext.Announcements.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
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
