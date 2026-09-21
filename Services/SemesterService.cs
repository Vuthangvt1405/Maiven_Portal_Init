using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Repository;


namespace Maiven_Portal_Managment.Services;

public sealed class SemesterService(SemesterRepository semesterRepository)
{
    public async Task<CreateSemesterResponse> CreateSemesterAsync(
        CreateSemesterRequest request)
    {
        var semester = new Semester
        {
            AcademicYearId = request.AcademicYearId,
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var createdSemester = await semesterRepository.CreateAsync(semester);

        return new CreateSemesterResponse
        {
            Id = createdSemester.Id,
            AcademicYearId = createdSemester.AcademicYearId,
            Name = createdSemester.Name,
            StartDate = createdSemester.StartDate,
            EndDate = createdSemester.EndDate
        };
    }
}