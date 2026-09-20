using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("USER_ROLES", table =>
            table.HasCheckConstraint("CK_USER_ROLES_status", "[status] IN ('ACTIVE', 'INACTIVE')"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.RoleId).HasColumnName("role_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasColumnType("varchar(10)").HasMaxLength(10).IsRequired();
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique().HasDatabaseName("UX_USER_ROLES_user_id_role_id");
        builder.HasIndex(x => x.RoleId).HasDatabaseName("IX_USER_ROLES_role_id");
        builder.HasIndex(x => new { x.IsDeleted, x.Status }).HasDatabaseName("IX_USER_ROLES_isDelete_status");
        builder.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.NoAction);
    }
}
