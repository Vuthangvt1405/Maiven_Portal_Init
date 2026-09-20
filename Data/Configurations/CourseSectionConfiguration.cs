using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public class CourseSectionConfiguration : IEntityTypeConfiguration<CourseSection>
{
    public void Configure(EntityTypeBuilder<CourseSection> builder)
    {
        builder.ToTable("COURSE_SECTIONS", table =>
        {
            table.HasCheckConstraint("CK_COURSE_SECTIONS_day_of_week", "[day_of_week] IN ('MONDAY', 'TUESDAY', 'WEDNESDAY', 'THURSDAY', 'FRIDAY', 'SATURDAY', 'SUNDAY')");
            table.HasCheckConstraint("CK_COURSE_SECTIONS_status", "[status] IN ('OPEN', 'COMPLETED', 'CANCELLED')");
            table.HasCheckConstraint("CK_COURSE_SECTIONS_capacity", "[capacity] >= 0");
            table.HasCheckConstraint("CK_COURSE_SECTIONS_time_range", "[start_time] < [end_time]");
            table.HasCheckConstraint("CK_COURSE_SECTIONS_date_range", "[start_date] <= [end_date]");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.CourseId).HasColumnName("course_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.SemesterId).HasColumnName("semester_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.TeacherUserRoleId).HasColumnName("teacher_user_role_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.SectionCode).HasColumnName("section_code").HasColumnType("varchar(50)").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Capacity).HasColumnName("capacity").HasColumnType("int").IsRequired();
        builder.Property(x => x.DayOfWeek).HasColumnName("day_of_week").HasConversion<string>().HasColumnType("varchar(10)").HasMaxLength(10).IsRequired();
        builder.Property(x => x.StartTime).HasColumnName("start_time").HasColumnType("time").IsRequired();
        builder.Property(x => x.EndTime).HasColumnName("end_time").HasColumnType("time").IsRequired();
        builder.Property(x => x.StartDate).HasColumnName("start_date").HasColumnType("date").IsRequired();
        builder.Property(x => x.EndDate).HasColumnName("end_date").HasColumnType("date").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasColumnType("varchar(10)").HasMaxLength(10).IsRequired();
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(x => new { x.SemesterId, x.SectionCode }).IsUnique().HasDatabaseName("UX_COURSE_SECTIONS_semester_id_section_code");
        builder.HasIndex(x => x.CourseId).HasDatabaseName("IX_COURSE_SECTIONS_course_id");
        builder.HasIndex(x => x.TeacherUserRoleId).HasDatabaseName("IX_COURSE_SECTIONS_teacher_user_role_id");
        builder.HasIndex(x => new { x.IsDeleted, x.Status }).HasDatabaseName("IX_COURSE_SECTIONS_isDelete_status");
        builder.HasOne(x => x.Course).WithMany(x => x.CourseSections).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Semester).WithMany(x => x.CourseSections).HasForeignKey(x => x.SemesterId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.TeacherUserRole).WithMany(x => x.TaughtCourseSections).HasForeignKey(x => x.TeacherUserRoleId).OnDelete(DeleteBehavior.NoAction);
    }
}
