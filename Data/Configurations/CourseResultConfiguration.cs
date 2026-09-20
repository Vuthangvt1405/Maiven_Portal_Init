using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public class CourseResultConfiguration : IEntityTypeConfiguration<CourseResult>
{
    public void Configure(EntityTypeBuilder<CourseResult> builder)
    {
        builder.ToTable("COURSE_RESULTS", table =>
        {
            table.HasCheckConstraint("CK_COURSE_RESULTS_final_score", "[final_score] >= 0 AND [final_score] <= 100");
            table.HasCheckConstraint("CK_COURSE_RESULTS_grade_point", "[grade_point] >= 0");
            table.HasCheckConstraint("CK_COURSE_RESULTS_result_status", "[result_status] IN ('PASS', 'FAIL', 'INCOMPLETE')");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.EnrollmentId).HasColumnName("enrollment_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.FinalScore).HasColumnName("final_score").HasColumnType("decimal(5,2)").HasPrecision(5, 2);
        builder.Property(x => x.LetterGrade).HasColumnName("letter_grade").HasColumnType("varchar(10)").HasMaxLength(10);
        builder.Property(x => x.GradePoint).HasColumnName("grade_point").HasColumnType("decimal(4,2)").HasPrecision(4, 2);
        builder.Property(x => x.ResultStatus).HasColumnName("result_status").HasConversion<string>().HasColumnType("varchar(10)").HasMaxLength(10);
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(x => x.EnrollmentId).IsUnique().HasDatabaseName("UX_COURSE_RESULTS_enrollment_id");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_COURSE_RESULTS_isDelete");
        builder.HasOne(x => x.Enrollment).WithOne(x => x.CourseResult).HasForeignKey<CourseResult>(x => x.EnrollmentId).OnDelete(DeleteBehavior.NoAction);
    }
}
