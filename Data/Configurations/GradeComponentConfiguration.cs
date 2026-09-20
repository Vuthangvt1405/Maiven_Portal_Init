using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public class GradeComponentConfiguration : IEntityTypeConfiguration<GradeComponent>
{
    public void Configure(EntityTypeBuilder<GradeComponent> builder)
    {
        builder.ToTable("GRADE_COMPONENTS", table =>
            table.HasCheckConstraint("CK_GRADE_COMPONENTS_weight", "[weight] >= 0 AND [weight] <= 100"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.SectionId).HasColumnName("section_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasColumnType("varchar(200)").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Weight).HasColumnName("weight").HasColumnType("decimal(5,2)").HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.HasIndex(x => new { x.SectionId, x.Name }).IsUnique().HasDatabaseName("UX_GRADE_COMPONENTS_section_id_name");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_GRADE_COMPONENTS_isDelete");
        builder.HasOne(x => x.Section).WithMany(x => x.GradeComponents).HasForeignKey(x => x.SectionId).OnDelete(DeleteBehavior.NoAction);
    }
}
