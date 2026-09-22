using Microsoft.EntityFrameworkCore;
using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;

namespace Maiven_Portal_Managment.Repository;

public sealed class CourseResultRepository(AppDbContext dbContext)
{
    public async Task<CourseSection?> GetCourseSectionByIdAsync(long sectionId, CancellationToken cancellationToken)
    {
        return await dbContext.Set<CourseSection>()
            .FirstOrDefaultAsync(x => x.Id == sectionId && !x.IsDeleted, cancellationToken);
    }

    public async Task<List<GradeComponent>> GetComponentsBySectionIdAsync(long sectionId, CancellationToken cancellationToken)
    {
        return await dbContext.Set<GradeComponent>()
            .Where(x => x.SectionId == sectionId && !x.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Enrollment>> GetEnrollmentsWithScoresAndGradeComponentsAsync(long sectionId, CancellationToken cancellationToken)
    {
        return await dbContext.Set<Enrollment>()
            .Include(e => e.StudentScores)
                .ThenInclude(s => s.Component)
            .Include(e => e.CourseResult)
            .Where(e => e.SectionId == sectionId)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}