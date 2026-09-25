using System.ComponentModel.DataAnnotations;

namespace Maiven_Portal_Managment.Dtos.request;

public sealed record UpdateStudentScoreRequest
{
    [Range(typeof(decimal), "0", "100")]
    public decimal Score { get; init; }
}
