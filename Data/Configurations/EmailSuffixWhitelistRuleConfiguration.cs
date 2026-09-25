using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public sealed class EmailSuffixWhitelistRuleConfiguration : IEntityTypeConfiguration<EmailSuffixWhitelistRule>
{
    public void Configure(EntityTypeBuilder<EmailSuffixWhitelistRule> builder)
    {
        builder.ToTable("EMAIL_SUFFIX_WHITELIST");
        builder.HasKey(rule => rule.Id);
        builder.Property(rule => rule.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(rule => rule.Suffix).HasColumnName("suffix").HasColumnType("varchar(253)").HasMaxLength(253).IsRequired();
        builder.Property(rule => rule.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(rule => rule.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(rule => rule.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(rule => rule.Suffix)
            .IsUnique()
            .HasFilter("[isDelete] = 0")
            .HasDatabaseName("UX_EMAIL_SUFFIX_WHITELIST_suffix");
        builder.HasIndex(rule => rule.IsDeleted).HasDatabaseName("IX_EMAIL_SUFFIX_WHITELIST_isDelete");
    }
}
