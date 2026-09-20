using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("COURSES", table =>
        {
            table.HasCheckConstraint("CK_COURSES_status", "[status] IN ('ACTIVE', 'INACTIVE')");
            table.HasCheckConstraint("CK_COURSES_credits", "[credits] >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.CourseCode).HasColumnName("course_code").HasColumnType("varchar(50)").HasMaxLength(50).IsRequired();
        builder.Property(x => x.CourseName).HasColumnName("course_name").HasColumnType("varchar(200)").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Credits).HasColumnName("credits").HasColumnType("int").IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasColumnType("nvarchar(max)");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasColumnType("varchar(10)").HasMaxLength(10).IsRequired();
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(x => x.CourseCode).IsUnique().HasDatabaseName("UX_COURSES_course_code");
        builder.HasIndex(x => new { x.IsDeleted, x.Status }).HasDatabaseName("IX_COURSES_isDelete_status");
    }
}
