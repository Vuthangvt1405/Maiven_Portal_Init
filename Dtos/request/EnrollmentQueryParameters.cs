namespace Maiven_Portal_Managment.Dtos.request;

public sealed record EnrollmentQueryParameters
{
    public string? Search { get; set; }
    public long? UserId { get; set; }
    public long? SemesterId { get; set; }
    public long? AcademicYearId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}