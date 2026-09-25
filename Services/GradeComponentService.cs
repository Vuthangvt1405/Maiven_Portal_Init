using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Repository;

namespace Maiven_Portal_Managment.Services;

public sealed class GradeComponentService(
    CourseSectionRepository courseSectionRepository,
    GradeComponentRepository gradeComponentRepository)
{
    public async Task<IReadOnlyList<GradeComponentResponse>> GetByCourseSectionAsync(
        long courseSectionId,
        CancellationToken cancellationToken)
    {
        if (courseSectionId <= 0)
            throw new BadRequestException("Course section ID must be a positive number.");

        var section = await courseSectionRepository.GetByIdAsync(courseSectionId, cancellationToken);
        if (section is null)
            throw new NotFoundException("The course section could not be found.");

        var components = await gradeComponentRepository.GetBySectionIdAsync(
            courseSectionId,
            cancellationToken);

        return components.Select(ToResponse).ToArray();
    }

    public async Task<IReadOnlyList<GradeComponentResponse>> UpdateWeightsAsync(
        long courseSectionId,
        UpdateGradeComponentsRequest request,
        CancellationToken cancellationToken)
    {
        if (courseSectionId <= 0)
            throw new BadRequestException("Course section ID must be a positive number.");

        if (request.Components is null || request.Components.Count != 4)
            throw new BadRequestException("Exactly 4 grade components are required.");

        if (request.Components.Any(component => component.GradeComponentId <= 0))
            throw new BadRequestException("Grade component IDs must be positive numbers.");

        if (request.Components.Select(component => component.GradeComponentId).Distinct().Count() != 4)
            throw new BadRequestException("Grade component IDs must be unique.");

        if (request.Components.Any(component => component.Weight < 0m || component.Weight > 100m))
            throw new BadRequestException("Grade component weights must be between 0 and 100.");

        if (request.Components.Sum(component => component.Weight) != 100m)
            throw new BadRequestException("Grade component weights must total 100.");

        var section = await courseSectionRepository.GetByIdAsync(courseSectionId, cancellationToken);
        if (section is null)
            throw new NotFoundException("The course section could not be found.");

        var weights = request.Components.ToDictionary(
            component => component.GradeComponentId,
            component => component.Weight);

        IReadOnlyList<GradeComponent> components;
        try
        {
            components = await gradeComponentRepository.UpdateWeightsAsync(
                courseSectionId,
                weights,
                cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            throw new ConflictException(exception.Message);
        }

        return components.Select(ToResponse).ToArray();
    }

    private static GradeComponentResponse ToResponse(GradeComponent component) =>
        new(
            component.Id,
            component.SectionId,
            component.Name,
            component.Weight);
}