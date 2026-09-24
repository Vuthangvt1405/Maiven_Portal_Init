namespace Maiven_Portal_Managment.Data.Entities;

public class FaceCredential : EntityBase
{
    public long UserId { get; set; }
    //Can be null after soft delete
    public byte[]? Embedding { get; set; }
    public int EmbeddingDimension { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }

    public User User { get; set; } = null!;
}
