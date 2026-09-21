using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("USER_ROLES");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.RoleId).HasColumnName("role_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasDatabaseName("UX_USER_ROLES_user_id_active")
            .HasFilter("[isDelete] = 0");
        builder.HasIndex(x => x.RoleId).HasDatabaseName("IX_USER_ROLES_role_id");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_USER_ROLES_isDelete");
        builder.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.NoAction);
    }
}
