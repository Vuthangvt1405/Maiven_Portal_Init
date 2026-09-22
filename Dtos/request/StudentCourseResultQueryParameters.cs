namespace Maiven_Portal_Managment.Dtos.request;

public sealed record StudentCourseResultQueryParameters
{
    public long? AcademicYearId { get; set; }
    public long? SemesterId { get; set; }
    public long? CourseId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
