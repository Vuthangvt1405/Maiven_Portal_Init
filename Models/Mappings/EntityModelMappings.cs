using System.Collections.Concurrent;
using System.Reflection;
using Maiven_Portal_Managment.Data.Entities;

namespace Maiven_Portal_Managment.Models.Mappings;

public static class EntityModelMappings
{
    public static UserModel ToModel(this User entity) =>
        entity.ToModel<User, UserModel>();

    public static User ToNewEntity(this UserModel model, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        var entity = new User { PasswordHash = passwordHash };
        model.ApplyToEntity<UserModel, User>(entity);
        return entity;
    }

    public static void ApplyToEntity(this UserModel model, User entity) =>
        model.ApplyToEntity<UserModel, User>(entity);

    public static RoleModel ToModel(this Role entity) =>
        entity.ToModel<Role, RoleModel>();

    public static Role ToNewEntity(this RoleModel model) =>
        model.ToNewEntity<RoleModel, Role>();

    public static void ApplyToEntity(this RoleModel model, Role entity) =>
        model.ApplyToEntity<RoleModel, Role>(entity);

    public static UserRoleModel ToModel(this UserRole entity) =>
        entity.ToModel<UserRole, UserRoleModel>();

    public static UserRole ToNewEntity(this UserRoleModel model) =>
        model.ToNewEntity<UserRoleModel, UserRole>();

    public static void ApplyToEntity(this UserRoleModel model, UserRole entity) =>
        model.ApplyToEntity<UserRoleModel, UserRole>(entity);

    public static AcademicYearModel ToModel(this AcademicYear entity) =>
        entity.ToModel<AcademicYear, AcademicYearModel>();

    public static AcademicYear ToNewEntity(this AcademicYearModel model) =>
        model.ToNewEntity<AcademicYearModel, AcademicYear>();

    public static void ApplyToEntity(this AcademicYearModel model, AcademicYear entity) =>
        model.ApplyToEntity<AcademicYearModel, AcademicYear>(entity);

    public static SemesterModel ToModel(this Semester entity) =>
        entity.ToModel<Semester, SemesterModel>();

    public static Semester ToNewEntity(this SemesterModel model) =>
        model.ToNewEntity<SemesterModel, Semester>();

    public static void ApplyToEntity(this SemesterModel model, Semester entity) =>
        model.ApplyToEntity<SemesterModel, Semester>(entity);

    public static CourseModel ToModel(this Course entity) =>
        entity.ToModel<Course, CourseModel>();

    public static Course ToNewEntity(this CourseModel model) =>
        model.ToNewEntity<CourseModel, Course>();

    public static void ApplyToEntity(this CourseModel model, Course entity) =>
        model.ApplyToEntity<CourseModel, Course>(entity);

    public static CourseSectionModel ToModel(this CourseSection entity) =>
        entity.ToModel<CourseSection, CourseSectionModel>();

    public static CourseSection ToNewEntity(this CourseSectionModel model) =>
        model.ToNewEntity<CourseSectionModel, CourseSection>();

    public static void ApplyToEntity(this CourseSectionModel model, CourseSection entity) =>
        model.ApplyToEntity<CourseSectionModel, CourseSection>(entity);

    public static RegistrationPeriodModel ToModel(this RegistrationPeriod entity) =>
        entity.ToModel<RegistrationPeriod, RegistrationPeriodModel>();

    public static RegistrationPeriod ToNewEntity(this RegistrationPeriodModel model) =>
        model.ToNewEntity<RegistrationPeriodModel, RegistrationPeriod>();

    public static void ApplyToEntity(this RegistrationPeriodModel model, RegistrationPeriod entity) =>
        model.ApplyToEntity<RegistrationPeriodModel, RegistrationPeriod>(entity);

    public static EnrollmentModel ToModel(this Enrollment entity) =>
        entity.ToModel<Enrollment, EnrollmentModel>();

    public static Enrollment ToNewEntity(this EnrollmentModel model) =>
        model.ToNewEntity<EnrollmentModel, Enrollment>();

    public static void ApplyToEntity(this EnrollmentModel model, Enrollment entity) =>
        model.ApplyToEntity<EnrollmentModel, Enrollment>(entity);

    public static AnnouncementModel ToModel(this Announcement entity) =>
        entity.ToModel<Announcement, AnnouncementModel>();

    public static Announcement ToNewEntity(this AnnouncementModel model) =>
        model.ToNewEntity<AnnouncementModel, Announcement>();

    public static void ApplyToEntity(this AnnouncementModel model, Announcement entity) =>
        model.ApplyToEntity<AnnouncementModel, Announcement>(entity);

    public static GradeComponentModel ToModel(this GradeComponent entity) =>
        entity.ToModel<GradeComponent, GradeComponentModel>();

    public static GradeComponent ToNewEntity(this GradeComponentModel model) =>
        model.ToNewEntity<GradeComponentModel, GradeComponent>();

    public static void ApplyToEntity(this GradeComponentModel model, GradeComponent entity) =>
        model.ApplyToEntity<GradeComponentModel, GradeComponent>(entity);

    public static StudentScoreModel ToModel(this StudentScore entity) =>
        entity.ToModel<StudentScore, StudentScoreModel>();

    public static StudentScore ToNewEntity(this StudentScoreModel model) =>
        model.ToNewEntity<StudentScoreModel, StudentScore>();

    public static void ApplyToEntity(this StudentScoreModel model, StudentScore entity) =>
        model.ApplyToEntity<StudentScoreModel, StudentScore>(entity);

    public static CourseResultModel ToModel(this CourseResult entity) =>
        entity.ToModel<CourseResult, CourseResultModel>();

    public static CourseResult ToNewEntity(this CourseResultModel model) =>
        model.ToNewEntity<CourseResultModel, CourseResult>();

    public static void ApplyToEntity(this CourseResultModel model, CourseResult entity) =>
        model.ApplyToEntity<CourseResultModel, CourseResult>(entity);

    public static TModel ToModel<TEntity, TModel>(this TEntity entity)
        where TEntity : class
        where TModel : class, new()
    {
        ArgumentNullException.ThrowIfNull(entity);
        var model = new TModel();
        CopyMatchingProperties(entity, model, forApply: false);
        return model;
    }

    public static void ApplyToEntity<TModel, TEntity>(this TModel model, TEntity entity)
        where TModel : class
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entity);
        CopyMatchingProperties(model, entity, forApply: true);
    }

    public static TEntity ToNewEntity<TModel, TEntity>(this TModel model)
        where TModel : class
        where TEntity : class, new()
    {
        ArgumentNullException.ThrowIfNull(model);
        var entity = new TEntity();
        CopyMatchingProperties(model, entity, forApply: true);
        return entity;
    }

    private static readonly ConcurrentDictionary<(Type Source, Type Dest, bool ForApply), IReadOnlyList<PropertyMap>> MapCache = new();

    private sealed record PropertyMap(
        PropertyInfo Source,
        PropertyInfo Dest,
        bool NeedsEnumConversion);

    private static void CopyMatchingProperties(object source, object dest, bool forApply)
    {
        var key = (source.GetType(), dest.GetType(), forApply);
        var maps = MapCache.GetOrAdd(key, static k => BuildPropertyMaps(k.Source, k.Dest, k.ForApply));

        foreach (var map in maps)
        {
            var value = map.Source.GetValue(source);
            if (map.NeedsEnumConversion)
            {
                value = ConvertEnumByName(value, map.Dest.PropertyType);
            }

            map.Dest.SetValue(dest, value);
        }
    }

    private static IReadOnlyList<PropertyMap> BuildPropertyMaps(Type sourceType, Type destType, bool forApply)
    {
        var destProps = destType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, p => p, StringComparer.Ordinal);

        var maps = new List<PropertyMap>();
        foreach (var sourceProp in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!sourceProp.CanRead || sourceProp.GetIndexParameters().Length != 0)
            {
                continue;
            }

            if (forApply && (sourceProp.Name is nameof(EntityBase.Id)
                or nameof(EntityBase.IsDeleted)
                or nameof(EntityBase.CreatedAt)
                or nameof(EntityBase.UpdatedAt)))
            {
                continue;
            }

            if (!destProps.TryGetValue(sourceProp.Name, out var destProp))
            {
                continue;
            }

            if (destProp.GetIndexParameters().Length != 0)
            {
                continue;
            }

            if (destProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
            {
                maps.Add(new PropertyMap(sourceProp, destProp, NeedsEnumConversion: false));
            }
            else if (GetEnumType(sourceProp.PropertyType) is not null
                && GetEnumType(destProp.PropertyType) is not null)
            {
                maps.Add(new PropertyMap(sourceProp, destProp, NeedsEnumConversion: true));
            }
        }

        return maps;
    }

    private static Type? GetEnumType(Type type)
    {
        if (type.IsEnum)
        {
            return type;
        }

        var underlying = Nullable.GetUnderlyingType(type);
        return underlying is not null && underlying.IsEnum ? underlying : null;
    }

    private static object? ConvertEnumByName(object? value, Type destPropertyType)
    {
        if (value is null)
        {
            return null;
        }

        var destEnumType = GetEnumType(destPropertyType);
        if (destEnumType is null)
        {
            return value;
        }

        var name = value.ToString();
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        if (Enum.TryParse(destEnumType, name, ignoreCase: false, out var parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException(
            $"Enum value '{value}' has no matching value in {destEnumType.FullName}.");
    }
}
