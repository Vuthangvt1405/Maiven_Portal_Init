using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public sealed class PasswordResetRequestConfiguration : IEntityTypeConfiguration<PasswordResetRequest>
{
    public void Configure(EntityTypeBuilder<PasswordResetRequest> builder)
    {
        builder.ToTable("PASSWORD_RESET_REQUESTS", table =>
            table.HasCheckConstraint("CK_PASSWORD_RESET_REQUESTS_method", "[method] IN ('OTP', 'FACE')"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.Method).HasColumnName("method").HasColumnType("varchar(10)").HasMaxLength(10).IsRequired();
        builder.Property(x => x.OtpHash).HasColumnName("otp_hash").HasColumnType("varchar(500)").HasMaxLength(500);
        builder.Property(x => x.ResetTokenHash).HasColumnName("reset_token_hash").HasColumnType("varchar(64)").HasMaxLength(64);
        builder.Property(x => x.ExpiresAtUtc).HasColumnName("expires_at_utc").HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.VerifiedAtUtc).HasColumnName("verified_at_utc").HasColumnType("datetime2");
        builder.Property(x => x.ConsumedAtUtc).HasColumnName("consumed_at_utc").HasColumnType("datetime2");
        builder.Property(x => x.InvalidatedAtUtc).HasColumnName("invalidated_at_utc").HasColumnType("datetime2");
        builder.Property(x => x.OtpAttemptCount).HasColumnName("otp_attempt_count").HasColumnType("int").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(x => new { x.UserId, x.Method, x.CreatedAt }).HasDatabaseName("IX_PASSWORD_RESET_REQUESTS_user_method_created");
        builder.HasIndex(x => x.ResetTokenHash).HasDatabaseName("IX_PASSWORD_RESET_REQUESTS_token_hash");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_PASSWORD_RESET_REQUESTS_isDelete");
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
    }
}
