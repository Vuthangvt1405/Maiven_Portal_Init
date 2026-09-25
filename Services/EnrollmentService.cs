using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Repository;

namespace Maiven_Portal_Managment.Services;

public sealed class EnrollmentService(EnrollmentRepository repository)
{
    public async Task<(IReadOnlyList<EnrollmentResponse> Items, int TotalItems)> GetPagedAsync(
        EnrollmentQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await repository.GetPagedAsync(parameters, cancellationToken);
        return (result.Items.Select(ToResponse).ToArray(), result.TotalItems);
    }

    public async Task<EnrollmentResponse> GetByIdAsync(long id, CancellationToken cancellationToken) =>
        ToResponse(await repository.GetByIdAsync(id, cancellationToken) ??
            throw new NotFoundException("The enrollment could not be found."));

    public Task<EnrollmentResponse> CreateAsync(
        long userId,
        CreateEnrollmentRequest request,
        CancellationToken cancellationToken) =>
        CreateOneAsync(userId, request, cancellationToken);

    public async Task<EnrollmentBatchResponse> CreateBatchAsync(
        long userId,
        CreateEnrollmentsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Enrollments is null || request.Enrollments.Count == 0)
            throw new BadRequestException("At least one enrollment is required.");

        var results = new List<EnrollmentBatchItemResponse>(request.Enrollments.Count);
        foreach (var item in request.Enrollments)
        {
            try
            {
                var enrollment = await CreateOneAsync(userId, item, cancellationToken);
                results.Add(new(userId, item.SectionId, "success", null, enrollment));
            }
            catch (ConflictException exception)
            {
                results.Add(new(userId, item.SectionId, "failed", exception.Message, null));
            }
            catch (NotFoundException exception)
            {
                results.Add(new(userId, item.SectionId, "failed", exception.Message, null));
            }
            catch (BadRequestException exception)
            {
                results.Add(new(userId, item.SectionId, "failed", exception.Message, null));
            }
        }

        return new(results, results.Count(x => x.Status == "success"), results.Count(x => x.Status == "failed"));
    }

    public Task<EnrollmentResponse> UpdateAsync(long id, UpdateEnrollmentRequest request, CancellationToken cancellationToken) =>
        SaveAsync(id, request.UserId, request.SectionId, cancellationToken);

    public async Task DeleteAsync(long studentUserRoleId, long sectionId, CancellationToken cancellationToken)
    {
        if (!await repository.DeleteAsync(studentUserRoleId, sectionId, cancellationToken))
            throw new NotFoundException("The enrollment could not be found.");
    }

    private async Task<EnrollmentResponse> CreateOneAsync(
        long userId,
        CreateEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        if (userId <= 0 || request.SectionId <= 0)
            throw new BadRequestException("UserId and SectionId must be positive numbers.");

        var studentRole = await repository.GetStudentRoleAsync(userId, cancellationToken)
            ?? throw new NotFoundException("The specified student could not be found.");

        var entity = new Enrollment
        {
            StudentUserRoleId = studentRole.Id,
            SectionId = request.SectionId
        };

        var result = await repository.TryCreateAsync(entity, cancellationToken);
        if (result == EnrollmentRepository.CreateResult.Full)
            throw new ConflictException("The course section is full.");
        if (result == EnrollmentRepository.CreateResult.Conflict)
            throw new ConflictException("The enrollment could not be created because of a conflict.");

        return ToResponse(await repository.GetByIdAsync(entity.Id, cancellationToken)
            ?? throw new ConflictException("The enrollment was created but could not be loaded."));
    }

    private async Task<EnrollmentResponse> SaveAsync(long? id, long userId, long sectionId, CancellationToken cancellationToken)
    {
        if (userId <= 0 || sectionId <= 0)
            throw new BadRequestException("UserId and SectionId must be positive numbers.");

        var studentRole = await repository.GetStudentRoleAsync(userId, cancellationToken)
            ?? throw new NotFoundException("The specified student could not be found.");
        var section = await repository.GetSectionAsync(sectionId, cancellationToken)
            ?? throw new NotFoundException("The specified course section could not be found.");

        if (await repository.ExistsAsync(userId, sectionId, id, cancellationToken))
            throw new ConflictException("The student is already enrolled in this course section.");
        if (await repository.CountInSectionAsync(sectionId, id, cancellationToken) >= section.Capacity)
            throw new ConflictException("The course section has reached its capacity.");

        var entity = id.HasValue
            ? await repository.GetByIdAsync(id.Value, cancellationToken) ?? throw new NotFoundException("The enrollment could not be found.")
            : new Enrollment();
        entity.StudentUserRoleId = studentRole.Id;
        entity.SectionId = sectionId;

        var saved = id.HasValue
            ? await repository.UpdateAsync(entity, cancellationToken)
            : await repository.AddAsync(entity, cancellationToken);
        return ToResponse(await repository.GetByIdAsync(saved!.Id, cancellationToken) ?? saved);
    }

    private static EnrollmentResponse ToResponse(Enrollment x) => new(
        x.Id, x.StudentUserRole.UserId, x.StudentUserRoleId, x.StudentUserRole.User.FullName,
        x.StudentUserRole.User.Email, x.SectionId, x.Section.SectionCode, x.Section.CourseId,
        x.Section.Course.CourseCode, x.Section.Course.CourseName, x.Section.SemesterId,
        x.Section.Semester.Name, x.Section.Semester.AcademicYearId, x.Section.Semester.AcademicYear.Name,
        x.CreatedAt, x.UpdatedAt);
}