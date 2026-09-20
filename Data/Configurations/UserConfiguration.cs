using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("USERS", table =>
            table.HasCheckConstraint("CK_USERS_gender", "[gender] IN ('MALE', 'FEMALE', 'OTHER')"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.Email).HasColumnName("email").HasColumnType("varchar(320)").HasMaxLength(320).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").HasColumnType("varchar(500)").HasMaxLength(500).IsRequired();
        builder.Property(x => x.FullName).HasColumnName("full_name").HasColumnType("varchar(200)").HasMaxLength(200).IsRequired();
        builder.Property(x => x.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("date");
        builder.Property(x => x.Gender).HasColumnName("gender").HasConversion<string>().HasColumnType("varchar(10)").HasMaxLength(10);
        builder.Property(x => x.Phone).HasColumnName("phone").HasColumnType("varchar(30)").HasMaxLength(30);
        builder.Property(x => x.Address).HasColumnName("address").HasColumnType("varchar(500)").HasMaxLength(500);
        builder.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasColumnType("varchar(500)").HasMaxLength(500);
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UX_USERS_email");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_USERS_isDelete");
    }
}
