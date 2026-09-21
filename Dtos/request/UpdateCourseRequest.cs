using System.ComponentModel.DataAnnotations;
using Maiven_Portal_Managment.Models.Enums;

namespace Maiven_Portal_Managment.Dtos.request
{
    public sealed record UpdateCourseRequest
    {
        [Required]
        public string CourseName { get; set; } = string.Empty;
        [Required]
        [Range(1, 10)]
        public int Credits { get; set; }

        public string? Description { get; set; }
        [Required]
        [EnumDataType(typeof(ActiveStatus))]
        public ActiveStatus Status { get; set; }
    }
}
