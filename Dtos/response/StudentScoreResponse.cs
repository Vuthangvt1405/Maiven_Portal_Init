namespace Maiven_Portal_Managment.Dtos.response;

public sealed record StudentScoreResponse(
    long Id,
    long EnrollmentId,
    long ComponentId,
    decimal? Score,
    long UpdatedById,
    DateTime UpdatedAt
);
