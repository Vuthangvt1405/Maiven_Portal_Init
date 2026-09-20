using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public class RegistrationPeriodConfiguration : IEntityTypeConfiguration<RegistrationPeriod>
{
    public void Configure(EntityTypeBuilder<RegistrationPeriod> builder)
    {
        builder.ToTable("REGISTRATION_PERIODS", table =>
        {
            table.HasCheckConstraint("CK_REGISTRATION_PERIODS_status", "[status] IN ('UPCOMING', 'OPEN', 'CLOSED')");
            table.HasCheckConstraint("CK_REGISTRATION_PERIODS_date_range", "[start_at] < [end_at]");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.SemesterId).HasColumnName("semester_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.StartAt).HasColumnName("start_at").HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.EndAt).HasColumnName("end_at").HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasColumnType("varchar(10)").HasMaxLength(10).IsRequired();
        builder.Property(x => x.CreatedById).HasColumnName("created_by").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(x => x.SemesterId).HasDatabaseName("IX_REGISTRATION_PERIODS_semester_id");
        builder.HasIndex(x => x.CreatedById).HasDatabaseName("IX_REGISTRATION_PERIODS_created_by");
        builder.HasIndex(x => new { x.IsDeleted, x.Status }).HasDatabaseName("IX_REGISTRATION_PERIODS_isDelete_status");
        builder.HasOne(x => x.Semester).WithMany(x => x.RegistrationPeriods).HasForeignKey(x => x.SemesterId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.CreatedBy).WithMany(x => x.CreatedRegistrationPeriods).HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.NoAction);
    }
}
