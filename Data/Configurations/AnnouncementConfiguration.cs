using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("ANNOUNCEMENTS", table =>
            table.HasCheckConstraint("CK_ANNOUNCEMENTS_status", "[status] IN ('DRAFT', 'PUBLISHED', 'ARCHIVED')"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.CreatedById).HasColumnName("created_by").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.SectionId).HasColumnName("section_id").HasColumnType("bigint");
        builder.Property(x => x.Title).HasColumnName("title").HasColumnType("varchar(200)").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Content).HasColumnName("content").HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasColumnType("varchar(10)").HasMaxLength(10).IsRequired();
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(x => x.CreatedById).HasDatabaseName("IX_ANNOUNCEMENTS_created_by");
        builder.HasIndex(x => x.SectionId).HasDatabaseName("IX_ANNOUNCEMENTS_section_id");
        builder.HasIndex(x => new { x.IsDeleted, x.Status }).HasDatabaseName("IX_ANNOUNCEMENTS_isDelete_status");
        builder.HasOne(x => x.CreatedBy).WithMany(x => x.CreatedAnnouncements).HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Section).WithMany(x => x.Announcements).HasForeignKey(x => x.SectionId).OnDelete(DeleteBehavior.NoAction);
    }
}
