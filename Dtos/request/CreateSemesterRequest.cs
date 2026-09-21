namespace Maiven_Portal_Managment.Dtos.Request;

public class CreateSemesterRequest
{
    public long AcademicYearId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}