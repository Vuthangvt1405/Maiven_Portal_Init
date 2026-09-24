using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maiven_Portal_Managment.Data.Configurations;

public class FaceCredentialConfiguration : IEntityTypeConfiguration<FaceCredential>
{
    public void Configure(EntityTypeBuilder<FaceCredential> builder)
    {
        builder.ToTable("FACE_CREDENTIALS", table =>
        {
            table.HasCheckConstraint(
                "CK_FACE_CREDENTIALS_embedding_dimension",
                "[embedding_dimension] > 0");
            table.HasCheckConstraint(
                "CK_FACE_CREDENTIALS_embedding_length",
                "[embedding] IS NULL OR DATALENGTH([embedding]) = [embedding_dimension] * 4");
            table.HasCheckConstraint(
                "CK_FACE_CREDENTIALS_delete_state",
                "([isDelete] = 0 AND [embedding] IS NOT NULL AND [deleted_at] IS NULL) OR " +
                "([isDelete] = 1 AND [embedding] IS NULL AND [deleted_at] IS NOT NULL)");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("bigint").ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("bigint").IsRequired();
        builder.Property(x => x.Embedding).HasColumnName("embedding").HasColumnType("varbinary(max)");
        builder.Property(x => x.EmbeddingDimension).HasColumnName("embedding_dimension").HasColumnType("int").IsRequired();
        builder.Property(x => x.ModelName).HasColumnName("model_name").HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModelVersion).HasColumnName("model_version").HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("datetime2");
        builder.Property(x => x.IsDeleted).HasColumnName("isDelete").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasDatabaseName("UX_FACE_CREDENTIALS_user_id_active")
            .HasFilter("[isDelete] = 0");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_FACE_CREDENTIALS_isDelete");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
