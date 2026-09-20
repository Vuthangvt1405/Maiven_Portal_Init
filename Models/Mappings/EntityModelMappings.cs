using Maiven_Portal_Managment.Data.Entities;
using ModelAcademicPeriodStatus = Maiven_Portal_Managment.Models.Enums.AcademicPeriodStatus;
using ModelActiveStatus = Maiven_Portal_Managment.Models.Enums.ActiveStatus;
using ModelAnnouncementStatus = Maiven_Portal_Managment.Models.Enums.AnnouncementStatus;
using ModelCourseSectionStatus = Maiven_Portal_Managment.Models.Enums.CourseSectionStatus;
using ModelEnrollmentStatus = Maiven_Portal_Managment.Models.Enums.EnrollmentStatus;
using ModelGender = Maiven_Portal_Managment.Models.Enums.Gender;
using ModelRegistrationPeriodStatus = Maiven_Portal_Managment.Models.Enums.RegistrationPeriodStatus;
using ModelResultStatus = Maiven_Portal_Managment.Models.Enums.ResultStatus;
using ModelWeekDay = Maiven_Portal_Managment.Models.Enums.WeekDay;
using EntityAcademicPeriodStatus = Maiven_Portal_Managment.Data.Entities.Enums.AcademicPeriodStatus;
using EntityActiveStatus = Maiven_Portal_Managment.Data.Entities.Enums.ActiveStatus;
using EntityAnnouncementStatus = Maiven_Portal_Managment.Data.Entities.Enums.AnnouncementStatus;
using EntityCourseSectionStatus = Maiven_Portal_Managment.Data.Entities.Enums.CourseSectionStatus;
using EntityEnrollmentStatus = Maiven_Portal_Managment.Data.Entities.Enums.EnrollmentStatus;
using EntityGender = Maiven_Portal_Managment.Data.Entities.Enums.Gender;
using EntityRegistrationPeriodStatus = Maiven_Portal_Managment.Data.Entities.Enums.RegistrationPeriodStatus;
using EntityResultStatus = Maiven_Portal_Managment.Data.Entities.Enums.ResultStatus;
using EntityWeekDay = Maiven_Portal_Managment.Data.Entities.Enums.WeekDay;

namespace Maiven_Portal_Managment.Models.Mappings;

public static class EntityModelMappings
{
    public static UserModel ToModel(this User entity) => new()
    {
        Id = entity.Id,
        Email = entity.Email,
        FullName = entity.FullName,
        DateOfBirth = entity.DateOfBirth,
        Gender = entity.Gender is null ? null : MapEnum<ModelGender>(entity.Gender.Value),
        Phone = entity.Phone,
        Address = entity.Address,
        AvatarUrl = entity.AvatarUrl,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static User ToNewEntity(this UserModel model, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        var entity = new User { PasswordHash = passwordHash };
        model.ApplyToEntity(entity);
        return entity;
    }

    public static void ApplyToEntity(this UserModel model, User entity)
    {
        entity.Email = model.Email;
        entity.FullName = model.FullName;
        entity.DateOfBirth = model.DateOfBirth;
        entity.Gender = model.Gender is null ? null : MapEnum<EntityGender>(model.Gender.Value);
        entity.Phone = model.Phone;
        entity.Address = model.Address;
        entity.AvatarUrl = model.AvatarUrl;
    }

    public static RoleModel ToModel(this Role entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        Description = entity.Description,
        Status = MapEnum<ModelActiveStatus>(entity.Status),
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static Role ToNewEntity(this RoleModel model)
    {
        var entity = new Role();
        model.ApplyToEntity(entity);
        return entity;
    }

    public static void ApplyToEntity(this RoleModel model, Role entity)
    {
        entity.Code = model.Code;
        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.Status = MapEnum<EntityActiveStatus>(model.Status);
    }

    public static UserRoleModel ToModel(this UserRole entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        RoleId = entity.RoleId,
        Status = MapEnum<ModelActiveStatus>(entity.Status),
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static UserRole ToNewEntity(this UserRoleModel model)
    {
        var entity = new UserRole();
        model.ApplyToEntity(entity);
        return entity;
    }

    public static void ApplyToEntity(this UserRoleModel model, UserRole entity)
    {
        entity.UserId = model.UserId;
        entity.RoleId = model.RoleId;
        entity.Status = MapEnum<EntityActiveStatus>(model.Status);
    }

    public static AcademicYearModel ToModel(this AcademicYear entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = MapEnum<ModelAcademicPeriodStatus>(entity.Status),
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static AcademicYear ToNewEntity(this AcademicYearModel model)
    {
        var entity = new AcademicYear();
        model.ApplyToEntity(entity);
        return entity;
    }

    public static void ApplyToEntity(this AcademicYearModel model, AcademicYear entity)
    {
        entity.Name = model.Name;
        entity.StartDate = model.StartDate;
        entity.EndDate = model.EndDate;
        entity.Status = MapEnum<EntityAcademicPeriodStatus>(model.Status);
    }

    public static SemesterModel ToModel(this Semester entity) => new()
    {
        Id = entity.Id,
        AcademicYearId = entity.AcademicYearId,
        Name = entity.Name,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = MapEnum<ModelAcademicPeriodStatus>(entity.Status),
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static Semester ToNewEntity(this SemesterModel model)
    {
        var entity = new Semester();
        model.ApplyToEntity(entity);
        return entity;
    }

    public static void ApplyToEntity(this SemesterModel model, Semester entity)
    {
        entity.AcademicYearId = model.AcademicYearId;
        entity.Name = model.Name;
        entity.StartDate = model.StartDate;
        entity.EndDate = model.EndDate;
        entity.Status = MapEnum<EntityAcademicPeriodStatus>(model.Status);
    }

    public static CourseModel ToModel(this Course entity) => new()
    {
        Id = entity.Id,
        CourseCode = entity.CourseCode,
        CourseName = entity.CourseName,
        Credits = entity.Credits,
        Description = entity.Description,
        Status = MapEnum<ModelActiveStatus>(entity.Status),
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static Course ToNewEntity(this CourseModel model)
    {
        var entity = new Course();
        model.ApplyToEntity(entity);
        return entity;
    }

    public static void ApplyToEntity(this CourseModel model, Course entity)
    {
        entity.CourseCode = model.CourseCode;
        entity.CourseName = model.CourseName;
        entity.Credits = model.Credits;
        entity.Description = model.Description;
        entity.Status = MapEnum<EntityActiveStatus>(model.Status);
    }

    public static CourseSectionModel ToModel(this CourseSection entity) => new()
    {
        Id = entity.Id,
        CourseId = entity.CourseId,
        SemesterId = entity.SemesterId,
        TeacherUserRoleId = entity.TeacherUserRoleId,
        SectionCode = entity.SectionCode,
        Capacity = entity.Capacity,
        DayOfWeek = MapEnum<ModelWeekDay>(entity.DayOfWeek),
        StartTime = entity.StartTime,
        EndTime = entity.EndTime,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = MapEnum<ModelCourseSectionStatus>(entity.Status),
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static CourseSection ToNewEntity(this CourseSectionModel model)
    {
        var entity = new CourseSection();
        model.ApplyToEntity(entity);
        return entity;
    }

    public static void ApplyToEntity(this CourseSectionModel model, CourseSection entity)
    {
        entity.CourseId = model.CourseId;
        entity.SemesterId = model.SemesterId;
        entity.TeacherUserRoleId = model.TeacherUserRoleId;
        entity.SectionCode = model.SectionCode;
        entity.Capacity = model.Capacity;
        entity.DayOfWeek = MapEnum<EntityWeekDay>(model.DayOfWeek);
        entity.StartTime = model.StartTime;
        entity.EndTime = model.EndTime;
        entity.StartDate = model.StartDate;
        entity.EndDate = model.EndDate;
        entity.Status = MapEnum<EntityCourseSectionStatus>(model.Status);
    }

    public static RegistrationPeriodModel ToModel(this RegistrationPeriod entity) => new()
    {
        Id = entity.Id,
        SemesterId = entity.SemesterId,
        StartAt = entity.StartAt,
        EndAt = entity.EndAt,
        Status = MapEnum<ModelRegistrationPeriodStatus>(entity.Status),
        CreatedById = entity.CreatedById,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static RegistrationPeriod ToNewEntity(this RegistrationPeriodModel model)
    {
        var entity = new RegistrationPeriod();
        model.ApplyToEntity(entity);
        return entity;
    }

    public static void ApplyToEntity(this RegistrationPeriodModel model, RegistrationPeriod entity)
    {
        entity.SemesterId = model.SemesterId;
        entity.StartAt = model.StartAt;
        entity.EndAt = model.EndAt;
        entity.Status = MapEnum<EntityRegistrationPeriodStatus>(model.Status);
        entity.CreatedById = model.CreatedById;
    }

    public static EnrollmentModel ToModel(this Enrollment entity) => new()
    {
        Id = entity.Id,
        StudentUserRoleId = entity.StudentUserRoleId,
        SectionId = entity.SectionId,
        Status = MapEnum<ModelEnrollmentStatus>(entity.Status),
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static Enrollment ToNewEntity(this EnrollmentModel model)
    {
        var entity = new Enrollment();
        model.ApplyToEntity(entity);
        return entity;
    }

    public static void ApplyToEntity(this EnrollmentModel model, Enrollment entity)
    {
        entity.StudentUserRoleId = model.StudentUserRoleId;
        entity.SectionId = model.SectionId;
        entity.Status = MapEnum<EntityEnrollmentStatus>(model.Status);
    }

    public static AnnouncementModel ToModel(this Announcement entity) => new()
    {
        Id = entity.Id,
        CreatedById = entity.CreatedById,
        SectionId = entity.SectionId,
        Title = entity.Title,
        Content = entity.Content,
        Status = MapEnum<ModelAnnouncementStatus>(entity.Status),
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static Announcement ToNewEntity(this AnnouncementModel model)
    {
        var entity = new Announcement();
        model.ApplyToEntity(entity);
        return entity;
    }

    public static void ApplyToEntity(this AnnouncementModel model, Announcement entity)
    {
        entity.CreatedById = model.CreatedById;
        entity.SectionId = model.SectionId;
        entity.Title = model.Title;
        entity.Content = model.Content;
        entity.Status = MapEnum<EntityAnnouncementStatus>(model.Status);
    }

    public static GradeComponentModel ToModel(this GradeComponent entity) => new()
    {
        Id = entity.Id,
        SectionId = entity.SectionId,
        Name = entity.Name,
        Weight = entity.Weight,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static GradeComponent ToNewEntity(this GradeComponentModel model)
    {
        var entity = new GradeComponent();
        model.ApplyToEntity(entity);
        return entity;
    }

    public static void ApplyToEntity(this GradeComponentModel model, GradeComponent entity)
    {
        entity.SectionId = model.SectionId;
        entity.Name = model.Name;
        entity.Weight = model.Weight;
    }

    public static StudentScoreModel ToModel(this StudentScore entity) => new()
    {
        Id = entity.Id,
        EnrollmentId = entity.EnrollmentId,
        ComponentId = entity.ComponentId,
        Score = entity.Score,
        UpdatedById = entity.UpdatedById,
        IsDeleted = entity.IsDeleted,
        UpdatedAt = entity.UpdatedAt
    };

    public static StudentScore ToNewEntity(this StudentScoreModel model)
    {
        var entity = new StudentScore();
        model.ApplyToEntity(entity);
        return entity;
    }

    public static void ApplyToEntity(this StudentScoreModel model, StudentScore entity)
    {
        entity.EnrollmentId = model.EnrollmentId;
        entity.ComponentId = model.ComponentId;
        entity.Score = model.Score;
        entity.UpdatedById = model.UpdatedById;
    }

    public static CourseResultModel ToModel(this CourseResult entity) => new()
    {
        Id = entity.Id,
        EnrollmentId = entity.EnrollmentId,
        FinalScore = entity.FinalScore,
        LetterGrade = entity.LetterGrade,
        GradePoint = entity.GradePoint,
        ResultStatus = entity.ResultStatus is null ? null : MapEnum<ModelResultStatus>(entity.ResultStatus.Value),
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    public static CourseResult ToNewEntity(this CourseResultModel model)
    {
        var entity = new CourseResult();
        model.ApplyToEntity(entity);
        return entity;
    }

    public static void ApplyToEntity(this CourseResultModel model, CourseResult entity)
    {
        entity.EnrollmentId = model.EnrollmentId;
        entity.FinalScore = model.FinalScore;
        entity.LetterGrade = model.LetterGrade;
        entity.GradePoint = model.GradePoint;
        entity.ResultStatus = model.ResultStatus is null ? null : MapEnum<EntityResultStatus>(model.ResultStatus.Value);
    }

    private static TTarget MapEnum<TTarget>(Enum value)
        where TTarget : struct, Enum
    {
        if (Enum.TryParse<TTarget>(value.ToString(), ignoreCase: false, out var mappedValue))
        {
            return mappedValue;
        }

        throw new InvalidOperationException(
            $"Enum value '{value}' from {value.GetType().FullName} has no matching value in {typeof(TTarget).FullName}.");
    }
}
