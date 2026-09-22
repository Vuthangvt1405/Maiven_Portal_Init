namespace Maiven_Portal_Managment.Dtos.request;

public sealed record CreateEnrollmentsRequest(
    IReadOnlyList<CreateEnrollmentRequest> Enrollments);