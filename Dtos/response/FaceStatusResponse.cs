namespace Maiven_Portal_Managment.Dtos.Response;

public sealed class FaceStatusResponse
{
    public bool IsRegistered { get; set; }

    // UTC; null when the Student has no active face credential.
    public DateTime? RegisteredAt { get; set; }

    public string? ModelName { get; set; }
}
