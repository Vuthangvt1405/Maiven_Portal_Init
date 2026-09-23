namespace Maiven_Portal_Managment.Dtos.Response;

public sealed class FaceVerifyResponse
{
    public bool Registered { get; set; }

    public DateTime RegisteredAt { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string ModelVersion { get; set; } = string.Empty;
}