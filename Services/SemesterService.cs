using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Data.Entities.Enums;
using Maiven_Portal_Managment.Repository;

namespace Maiven_Portal_Managment.Services;

public sealed class SemesterService(
    SemesterRepository semesterRepository,
    AcademicYearRepository academicYearRepository)
{
    private const string AcademicYearNotFoundMessage =
        "The academic year could not be found.";
    private const string InactiveAcademicYearMessage =
        "Semesters can only be managed in an active academic year.";
    private const string OutsideAcademicYearMessage =
        "The semester date range must be within the selected academic year.";
    private const string DuplicateNameMessage =
        "A semester with the same name already exists in the academic year.";
    private const string OverlappingRangeMessage =
        "The semester date range overlaps an existing semester.";

    public async Task<SemesterResponse> CreateAsync(
        CreateSemesterRequest request,
        CancellationToken cancellationToken)
    {
        var entity = BuildEntity(
            request.AcademicYearId,
            request.Name,
            request.StartDate,
            request.EndDate);

        await ValidateAcademicYearAsync(entity, cancellationToken);
        await ValidateConflictsAsync(entity, null, cancellationToken);

        var created = await semesterRepository.CreateAsync(entity, cancellationToken);
        var response = ToResponse(created);
        return response;
    }

    public async Task<IReadOnlyList<SemesterResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var semesters = await semesterRepository.GetAllAsync(cancellationToken);
        var response = semesters.Select(ToResponse).ToArray();
        return response;
    }

    public async Task<SemesterResponse> UpdateAsync(
        long semesterId,
        UpdateSemesterRequest request,
        CancellationToken cancellationToken)
    {
        if (semesterId <= 0)
        {
            throw new BadRequestException(
                "SemesterId must be greater than zero.");
        }

        var existing = await semesterRepository.GetTrackedByIdAsync(
            semesterId,
            cancellationToken);
        if (existing is null)
        {
            throw new NotFoundException("The semester could not be found.");
        }

        var currentAcademicYear = await academicYearRepository.GetByIdAsync(
            existing.AcademicYearId,
            cancellationToken);
        if (currentAcademicYear is null)
        {
            throw new NotFoundException(AcademicYearNotFoundMessage);
        }

        if (currentAcademicYear.Status != AcademicPeriodStatus.ACTIVE)
        {
            throw new ConflictException(InactiveAcademicYearMessage);
        }

        var proposed = BuildEntity(
            request.AcademicYearId,
            request.Name,
            request.StartDate,
            request.EndDate);
        proposed.Id = semesterId;

        await ValidateAcademicYearAsync(proposed, cancellationToken);
        await ValidateConflictsAsync(proposed, semesterId, cancellationToken);

        existing.AcademicYearId = proposed.AcademicYearId;
        existing.Name = proposed.Name;
        existing.StartDate = proposed.StartDate;
        existing.EndDate = proposed.EndDate;

        await semesterRepository.SaveChangesAsync(cancellationToken);

        var response = ToResponse(existing);
        return response;
    }

    private async Task ValidateAcademicYearAsync(
        Semester entity,
        CancellationToken cancellationToken)
    {
        var academicYear = await academicYearRepository.GetByIdAsync(
            entity.AcademicYearId,
            cancellationToken);
        if (academicYear is null)
        {
            throw new NotFoundException(AcademicYearNotFoundMessage);
        }

        if (academicYear.Status != AcademicPeriodStatus.ACTIVE)
        {
            throw new ConflictException(InactiveAcademicYearMessage);
        }

        if (entity.StartDate < academicYear.StartDate ||
            entity.EndDate > academicYear.EndDate)
        {
            throw new ConflictException(OutsideAcademicYearMessage);
        }
    }

    private async Task ValidateConflictsAsync(
        Semester proposed,
        long? excludedSemesterId,
        CancellationToken cancellationToken)
    {
        var semesters = await semesterRepository.GetByAcademicYearIdAsync(
            proposed.AcademicYearId,
            cancellationToken);
        var otherSemesters = semesters.Where(semester =>
            semester.Id != excludedSemesterId);

        if (otherSemesters.Any(semester =>
                string.Equals(
                    semester.Name,
                    proposed.Name,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException(DuplicateNameMessage);
        }

        if (otherSemesters.Any(semester =>
                semester.StartDate <= proposed.EndDate &&
                semester.EndDate >= proposed.StartDate))
        {
            throw new ConflictException(OverlappingRangeMessage);
        }
    }

    private static Semester BuildEntity(
        long academicYearId,
        string? name,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        if (academicYearId <= 0)
        {
            throw new BadRequestException(
                "AcademicYearId must be greater than zero.");
        }

        var normalizedName = name?.Trim() ?? string.Empty;
        if (normalizedName.Length is < 1 or > 200)
        {
            throw new BadRequestException(
                "Semester name must contain between 1 and 200 characters after trimming.");
        }

        if (!startDate.HasValue || !endDate.HasValue)
        {
            throw new BadRequestException(
                "AcademicYearId, name, startDate, and endDate are required.");
        }

        if (startDate.Value > endDate.Value)
        {
            throw new BadRequestException(
                "Semester startDate must be on or before endDate.");
        }

        return new Semester
        {
            AcademicYearId = academicYearId,
            Name = normalizedName,
            StartDate = startDate.Value,
            EndDate = endDate.Value
        };
    }

    private static SemesterResponse ToResponse(Semester entity) => new()
    {
        Id = entity.Id,
        AcademicYearId = entity.AcademicYearId,
        Name = entity.Name,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        CreatedAt = AsUtc(entity.CreatedAt),
        UpdatedAt = AsUtc(entity.UpdatedAt)
    };

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
