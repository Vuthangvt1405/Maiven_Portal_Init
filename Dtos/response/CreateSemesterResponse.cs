namespace Maiven_Portal_Managment.Dtos.Response;

public class CreateSemesterResponse
{
    public long Id { get; set; }

    public long AcademicYearId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }
}