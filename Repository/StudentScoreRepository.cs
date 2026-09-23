using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class StudentScoreRepository(AppDbContext dbContext)
{
    public Task<StudentScore?> GetByIdWithSectionAsync(
        long studentScoreId,
        CancellationToken cancellationToken) =>
        dbContext.StudentScores
            .Include(studentScore => studentScore.Enrollment)
                .ThenInclude(enrollment => enrollment.Section)
            .SingleOrDefaultAsync(
                studentScore => studentScore.Id == studentScoreId,
                cancellationToken);

    public async Task<StudentScore> UpdateScoreAsync(
        StudentScore studentScore,
        decimal score,
        long updatedById,
        CancellationToken cancellationToken)
    {
        studentScore.Score = score;
        studentScore.UpdatedById = updatedById;
        studentScore.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return studentScore;
    }
}
