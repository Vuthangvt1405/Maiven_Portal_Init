namespace Maiven_Portal_Managment.Dtos.response;

public sealed record EnrollmentBatchItemResponse(
    long UserId,
    long SectionId,
    string Status,
    string? Reason,
    EnrollmentResponse? Enrollment);