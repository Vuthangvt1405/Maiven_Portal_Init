namespace Maiven_Portal_Managment.Dtos.request
{
    public sealed class CourseQueryParameters
    {
        public string? CourseCode { get; set; }

        public string? CourseName { get; set; }

        public int? Credits { get; set; }

        public int? Status { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        
    }

}
