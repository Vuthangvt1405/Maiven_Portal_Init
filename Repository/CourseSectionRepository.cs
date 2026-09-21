using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;

using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;

using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Models.Enums;
using Maiven_Portal_Managment.Models.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Repository;

public sealed class CourseSectionRepository(AppDbContext dbContext)
{
    public async Task<CourseSectionModel?> GetByIdAsync(
        long sectionId,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.CourseSections
            .AsNoTracking()
            .SingleOrDefaultAsync(
                s => s.Id == sectionId && !s.IsDeleted,
                cancellationToken);

        return entity?.ToModel();
    }

    public async Task<CourseSectionModel?> GetBySemesterAndCodeAsync(
        long semesterId,
        string sectionCode,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.CourseSections
            .AsNoTracking()
            .SingleOrDefaultAsync(
                s => s.SemesterId == semesterId &&
                     s.SectionCode == sectionCode &&
                     !s.IsDeleted,
                cancellationToken);

        return entity?.ToModel();
    }

    public async Task<(IReadOnlyList<CourseSectionModel> Items, int TotalItems)> GetPagedAsync(
        long? courseId,
        long? semesterId,
        long? teacherUserRoleId,
        string? sectionCode,
        WeekDay? dayOfWeek,
        CourseSectionStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.CourseSections
            .AsNoTracking()
            .Where(s => !s.IsDeleted);

        if (courseId.HasValue)
        {
            query = query.Where(s => s.CourseId == courseId.Value);
        }

        if (semesterId.HasValue)
        {
            query = query.Where(s => s.SemesterId == semesterId.Value);
        }

        if (teacherUserRoleId.HasValue)
        {
            query = query.Where(s => s.TeacherUserRoleId == teacherUserRoleId.Value);
        }

        if (!string.IsNullOrWhiteSpace(sectionCode))
        {
            query = query.Where(s => s.SectionCode.Contains(sectionCode));
        }

        if (dayOfWeek.HasValue)
        {
            query = query.Where(s => (int)s.DayOfWeek == (int)dayOfWeek.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => (int)s.Status == (int)status.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var entities = await query
            .OrderBy(s => s.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = entities
            .Select(s => s.ToModel())
            .ToArray();

        return (items, totalItems);
    }

    public async Task<(IReadOnlyList<CourseSectionModel> Items, int TotalItems)> GetPagedForTeacherAsync(
    long teacherUserRoleId,
    CourseSectionQueryParameters parameters,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken)
    {
        var query = dbContext.CourseSections
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.TeacherUserRoleId == teacherUserRoleId);

        return await ApplyFiltersAndPagingAsync(query, parameters, pageNumber, pageSize, cancellationToken);
    }

    public async Task<(IReadOnlyList<CourseSectionModel> Items, int TotalItems)> GetPagedForStudentAsync(
        long studentUserRoleId,
        CourseSectionQueryParameters parameters,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.CourseSections
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.Enrollments.Any(e => !e.IsDeleted
                                                            && e.StudentUserRoleId == studentUserRoleId
                                                            && !e.StudentUserRole.IsDeleted));

        return await ApplyFiltersAndPagingAsync(query, parameters, pageNumber, pageSize, cancellationToken);
    }

    private static async Task<(IReadOnlyList<CourseSectionModel> Items, int TotalItems)> ApplyFiltersAndPagingAsync(
        IQueryable<CourseSection> query,
        CourseSectionQueryParameters parameters,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (parameters.CourseId.HasValue)
        {
            query = query.Where(s => s.CourseId == parameters.CourseId.Value);
        }

        if (parameters.SemesterId.HasValue)
        {
            query = query.Where(s => s.SemesterId == parameters.SemesterId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SectionCode))
        {
            query = query.Where(s => s.SectionCode.Contains(parameters.SectionCode));
        }

        if (parameters.DayOfWeek.HasValue)
        {
            query = query.Where(s => (int)s.DayOfWeek == (int)parameters.DayOfWeek.Value);
        }

        if (parameters.Status.HasValue)
        {
            query = query.Where(s => (int)s.Status == (int)parameters.Status.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var entities = await query
            .OrderBy(s => s.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(s => s.ToModel()).ToArray();

        return (items, totalItems);
    }

    public async Task<CourseSectionModel> AddAsync(
        CourseSectionModel model,
        CancellationToken cancellationToken)
    {
        var entity = model.ToNewEntity();
        var defaultComponents = Enum.GetValues<DefaultGradeComponent>()
        .Select(type => new GradeComponent
        {
            Name = type.GetDescription(),
            Weight = 0m
        })
        .ToList();
        
        foreach (var component in defaultComponents)
        {
            entity.GradeComponents.Add(component);
        }

        dbContext.CourseSections.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }

    public async Task<CourseSectionModel?> UpdateAsync(
        CourseSectionModel model,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.CourseSections
            .SingleOrDefaultAsync(
                s => s.Id == model.Id && !s.IsDeleted,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        model.ApplyToEntity(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToModel();
    }

    public async Task<(CourseSectionModel? Section, IReadOnlyList<StudentInCourseSectionResponse> Students, int TotalItems)> GetCourseSectionDetailsWithStudentsAsync(
        long teacherUserRoleId,
        long sectionId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var sectionEntity = await dbContext.CourseSections
            .AsNoTracking()
            .SingleOrDefaultAsync(
                s => s.Id == sectionId && s.TeacherUserRoleId == teacherUserRoleId && !s.IsDeleted,
                cancellationToken);

        if (sectionEntity == null)
        {
            return (null, Array.Empty<StudentInCourseSectionResponse>(), 0);
        }

        var enrollmentsQuery = dbContext.Enrollments
            .AsNoTracking()
            .Where(e => e.SectionId == sectionId && !e.IsDeleted && !e.StudentUserRole.IsDeleted);

        var totalItems = await enrollmentsQuery.CountAsync(cancellationToken);

        var students = await enrollmentsQuery
            .OrderBy(e => e.StudentUserRole.User.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new StudentInCourseSectionResponse(
                e.Id,
                e.StudentUserRoleId,
                e.StudentUserRole.User.FullName,
                e.StudentUserRole.User.Email,
                e.StudentScores
                    .Select(sc => new StudentScoreDetailResponse(
                        sc.ComponentId,
                        sc.Component.Name,
                        sc.Component.Weight,
                        sc.Score
                    ))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);

        return (sectionEntity.ToModel(), students, totalItems);
    }

}