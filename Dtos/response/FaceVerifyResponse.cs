namespace Maiven_Portal_Managment.Dtos.Response;

public sealed class FaceVerifyResponse
{
    public bool Registered { get; set; }

    // UTC; returned after the credential has been saved successfully.
    public DateTime RegisteredAt { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string ModelVersion { get; set; } = string.Empty;
}
