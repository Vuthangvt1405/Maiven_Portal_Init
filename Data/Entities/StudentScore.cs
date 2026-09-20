namespace Maiven_Portal_Managment.Data.Entities;

public class StudentScore
{
    public long Id { get; set; }
    public long EnrollmentId { get; set; }
    public long ComponentId { get; set; }
    public decimal? Score { get; set; }
    public long UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Enrollment Enrollment { get; set; } = null!;
    public GradeComponent Component { get; set; } = null!;
    public User UpdatedBy { get; set; } = null!;
}
