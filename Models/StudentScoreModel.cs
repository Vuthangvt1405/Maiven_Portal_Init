namespace Maiven_Portal_Managment.Models;

public sealed class StudentScoreModel
{
    public long Id { get; set; }
    public long EnrollmentId { get; set; }
    public long ComponentId { get; set; }
    public decimal? Score { get; set; }
    public long UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime UpdatedAt { get; set; }
}
