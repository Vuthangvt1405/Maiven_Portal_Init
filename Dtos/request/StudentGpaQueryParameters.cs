namespace Maiven_Portal_Managment.Dtos.request;

public sealed record StudentGpaQueryParameters
{
    public long? AcademicYearId { get; init; }
}
