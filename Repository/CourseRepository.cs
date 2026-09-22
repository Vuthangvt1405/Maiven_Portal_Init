using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository
{
    public sealed class CourseRepository(AppDbContext dbContext)
    {
        public async Task<Course?> GetByIdAsync(
            long courseId,
            CancellationToken cancellationToken)
        {
            var entity = await dbContext.Courses
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    course => course.Id == courseId &&
                              !course.IsDeleted,
                    cancellationToken);

            return entity;
        }

        public async Task<Course?> GetByCourseCodeAsync(
            string courseCode,
            CancellationToken cancellationToken)
        {
            var entity = await dbContext.Courses
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    course => course.CourseCode == courseCode &&
                              !course.IsDeleted,
                    cancellationToken);

            return entity;
        }

        public async Task<(IReadOnlyList<Course> Items, int TotalItems)> GetPagedAsync(
            string? courseCode,
            string? courseName,
            int? credits,
            int? status,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var query = dbContext.Courses
                .AsNoTracking()
                .Where(course => !course.IsDeleted);

            if (!string.IsNullOrWhiteSpace(courseCode))
            {
                query = query.Where(course =>
                    course.CourseCode.Contains(courseCode));
            }

            if (!string.IsNullOrWhiteSpace(courseName))
            {
                query = query.Where(course =>
                    course.CourseName.Contains(courseName));
            }

            if (credits.HasValue)
            {
                query = query.Where(course =>
                    course.Credits == credits.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(course =>
                    (int)course.Status == status.Value);
            }

            var totalItems = await query.CountAsync(cancellationToken);

            var entities = await query
                .OrderBy(course => course.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (entities, totalItems);
        }

        public async Task<Course> AddAsync(
            Course entity,
            CancellationToken cancellationToken)
        {
            dbContext.Courses.Add(entity);

            await dbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }

        public async Task<Course?> UpdateAsync(
            Course values,
            CancellationToken cancellationToken)
        {
            var entity = await dbContext.Courses
                .SingleOrDefaultAsync(
                    course => course.Id == values.Id &&
                              !course.IsDeleted,
                    cancellationToken);

            if (entity is null)
            {
                return null;
            }

            entity.CourseName = values.CourseName;
            entity.Credits = values.Credits;
            entity.Description = values.Description;
            entity.Status = values.Status;

            await dbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }

        public async Task<bool> DeleteAsync(
            long courseId,
            CancellationToken cancellationToken)
        {
            var entity = await dbContext.Courses
                .SingleOrDefaultAsync(
                    course => course.Id == courseId &&
                              !course.IsDeleted,
                    cancellationToken);

            if (entity is null)
            {
                return false;
            }

            entity.IsDeleted = true;

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
