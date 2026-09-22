namespace Maiven_Portal_Managment.Dtos.response;

public sealed record EnrollmentBatchResponse(
    IReadOnlyList<EnrollmentBatchItemResponse> Results,
    int SuccessCount,
    int FailedCount);