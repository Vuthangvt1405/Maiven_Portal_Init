using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public class StudentScoreConfiguration : IEntityTypeConfiguration<StudentScore>
{
    public void Configure(EntityTypeBuilder<StudentScore> builder)
    {
        builder.ToTable("STUDENT_SCORES", table =>
            table.HasCheckConstraint("CK_STUDENT_SCORES_score", "[score] >= 0 AND [score] <= 100"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.EnrollmentId).HasColumnName("enrollment_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.ComponentId).HasColumnName("component_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.Score).HasColumnName("score").HasColumnType("decimal(5,2)").HasPrecision(5, 2);
        builder.Property(x => x.UpdatedById).HasColumnName("updated_by").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(x => new { x.EnrollmentId, x.ComponentId }).IsUnique().HasDatabaseName("UX_STUDENT_SCORES_enrollment_id_component_id");
        builder.HasIndex(x => x.ComponentId).HasDatabaseName("IX_STUDENT_SCORES_component_id");
        builder.HasIndex(x => x.UpdatedById).HasDatabaseName("IX_STUDENT_SCORES_updated_by");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_STUDENT_SCORES_isDelete");
        builder.HasOne(x => x.Enrollment).WithMany(x => x.StudentScores).HasForeignKey(x => x.EnrollmentId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Component).WithMany(x => x.StudentScores).HasForeignKey(x => x.ComponentId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.UpdatedBy).WithMany(x => x.UpdatedStudentScores).HasForeignKey(x => x.UpdatedById).OnDelete(DeleteBehavior.NoAction);
    }
}
