using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("ENROLLMENTS", table =>
            table.HasCheckConstraint("CK_ENROLLMENTS_status", "[status] IN ('ENROLLED', 'DROPPED', 'COMPLETED')"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.StudentUserRoleId).HasColumnName("student_user_role_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.SectionId).HasColumnName("section_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasColumnType("varchar(10)").HasMaxLength(10).IsRequired();
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(x => new { x.StudentUserRoleId, x.SectionId }).IsUnique().HasDatabaseName("UX_ENROLLMENTS_student_user_role_id_section_id");
        builder.HasIndex(x => x.SectionId).HasDatabaseName("IX_ENROLLMENTS_section_id");
        builder.HasIndex(x => new { x.IsDeleted, x.Status }).HasDatabaseName("IX_ENROLLMENTS_isDelete_status");
        builder.HasOne(x => x.StudentUserRole).WithMany(x => x.Enrollments).HasForeignKey(x => x.StudentUserRoleId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Section).WithMany(x => x.Enrollments).HasForeignKey(x => x.SectionId).OnDelete(DeleteBehavior.NoAction);
    }
}
