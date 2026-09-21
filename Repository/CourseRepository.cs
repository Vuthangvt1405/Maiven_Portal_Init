using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Models.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository
{
    public sealed class CourseRepository(AppDbContext dbContext)
    {
        public async Task<CourseModel?> GetByIdAsync(
            long courseId,
            CancellationToken cancellationToken)
        {
            var entity = await dbContext.Courses
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    course => course.Id == courseId &&
                              !course.IsDeleted,
                    cancellationToken);

            return entity?.ToModel();
        }

        public async Task<CourseModel?> GetByCourseCodeAsync(
            string courseCode,
            CancellationToken cancellationToken)
        {
            var entity = await dbContext.Courses
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    course => course.CourseCode == courseCode &&
                              !course.IsDeleted,
                    cancellationToken);

            return entity?.ToModel();
        }

        public async Task<(IReadOnlyList<CourseModel> Items, int TotalItems)> GetPagedAsync(
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

            var items = entities
                .Select(course => course.ToModel())
                .ToArray();

            return (items, totalItems);
        }

        public async Task<CourseModel> AddAsync(
            CourseModel model,
            CancellationToken cancellationToken)
        {
            var entity = model.ToNewEntity();

            dbContext.Courses.Add(entity);

            await dbContext.SaveChangesAsync(cancellationToken);

            return entity.ToModel();
        }

        public async Task<CourseModel?> UpdateAsync(
            CourseModel model,
            CancellationToken cancellationToken)
        {
            var entity = await dbContext.Courses
                .SingleOrDefaultAsync(
                    course => course.Id == model.Id &&
                              !course.IsDeleted,
                    cancellationToken);

            if (entity is null)
            {
                return null;
            }

            model.ApplyToEntity(entity);

            await dbContext.SaveChangesAsync(cancellationToken);

            return entity.ToModel();
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
