namespace Maiven_Portal_Managment.Dtos.response;

public sealed record StudentGpaResponse
{
    public IReadOnlyList<AcademicYearGpaResponse> AcademicYears { get; init; } = [];
}

public sealed record AcademicYearGpaResponse
{
    public long AcademicYearId { get; init; }
    public string AcademicYearName { get; init; } = string.Empty;
    public decimal? CumulativeGpa { get; init; }
    public int CumulativeCompletedCredits { get; init; }
    public IReadOnlyList<SemesterGpaResponse> Semesters { get; init; } = [];
}

public sealed record SemesterGpaResponse
{
    public long SemesterId { get; init; }
    public string SemesterName { get; init; } = string.Empty;
    public decimal? Gpa { get; init; }
    public int CompletedCredits { get; init; }
}
