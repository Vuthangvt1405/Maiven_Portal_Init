using Maiven_Portal_Managment.Common;

namespace Maiven_Portal_Managment.Services;

public sealed class FaceEmbeddingResult
{
    private FaceEmbeddingResult(
        FaceFrameStatus status,
        float[]? embedding)
    {
        Status = status;
        Embedding = embedding;
    }

    public bool Success =>
        Status == FaceFrameStatus.Success &&
        Embedding is { Length: > 0 };

    public FaceFrameStatus Status { get; }

    public float[]? Embedding { get; }

    public static FaceEmbeddingResult Succeeded(float[] embedding)
    {
        ArgumentNullException.ThrowIfNull(embedding);
        return new FaceEmbeddingResult(FaceFrameStatus.Success, embedding);
    }

    public static FaceEmbeddingResult Rejected(FaceFrameStatus status)
    {
        if (status == FaceFrameStatus.Success)
        {
            throw new ArgumentException(
                "A rejected face frame cannot have Success status.",
                nameof(status));
        }

        return new FaceEmbeddingResult(status, null);
    }
}
