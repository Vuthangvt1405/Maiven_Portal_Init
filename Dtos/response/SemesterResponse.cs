namespace Maiven_Portal_Managment.Dtos.Response;

public sealed class SemesterResponse
{
    public long Id { get; init; }

    public AcademicYearResponse AcademicYear { get; init; } = null!;

    public string Name { get; init; } = string.Empty;

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}
