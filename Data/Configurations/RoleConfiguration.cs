using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("ROLES");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.Code).HasColumnName("code").HasColumnType("varchar(50)").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasColumnType("varchar(200)").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasColumnType("nvarchar(max)");
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("UX_ROLES_code");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_ROLES_isDelete");

        var seedTimestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new Role
            {
                Id = SystemRoles.Student.Id,
                Code = SystemRoles.Student.Code,
                Name = SystemRoles.Student.Name,
                Description = SystemRoles.Student.Description,
                IsDeleted = false,
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            },
            new Role
            {
                Id = SystemRoles.Teacher.Id,
                Code = SystemRoles.Teacher.Code,
                Name = SystemRoles.Teacher.Name,
                Description = SystemRoles.Teacher.Description,
                IsDeleted = false,
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            },
            new Role
            {
                Id = SystemRoles.Admin.Id,
                Code = SystemRoles.Admin.Code,
                Name = SystemRoles.Admin.Name,
                Description = SystemRoles.Admin.Description,
                IsDeleted = false,
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            });
    }
}
