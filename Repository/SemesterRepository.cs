using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;

namespace Maiven_Portal_Managment.Repository;

public sealed class SemesterRepository(AppDbContext dbContext)
{
    public async Task<Semester> CreateAsync(Semester semester)
    {
        await dbContext.Semesters.AddAsync(semester);
        await dbContext.SaveChangesAsync();

        return semester;
    }
}