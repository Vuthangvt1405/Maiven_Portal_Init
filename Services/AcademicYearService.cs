using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Data.Entities.Enums;
using Maiven_Portal_Managment.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Services;

public sealed class AcademicYearService(
    AcademicYearRepository academicYearRepository)
{
    private const string DuplicateNameMessage =
        "An academic year with the same name already exists.";
    private const string OverlappingRangeMessage =
        "The academic-year date range overlaps an existing academic year.";
    private const string ActiveYearMessage =
        "Only one active academic year is allowed.";
    private const string SemesterRangeMessage =
        "The academic-year date range must contain all of its existing semesters.";

    public async Task<AcademicYearResponse> CreateAsync(
        CreateAcademicYearRequest request,
        CancellationToken cancellationToken)
    {
        var entity = BuildEntity(
            request.Name,
            request.StartDate,
            request.EndDate,
            request.Status);

        await ValidateConflictsAsync(entity, null, cancellationToken);

        try
        {
            var created = await academicYearRepository.AddAsync(entity, cancellationToken);
            var response = ToResponse(created);
            return response;
        }
        catch (DbUpdateException exception)
        {
            var conflict = TranslateUniqueConstraintException(exception);
            if (conflict is not null)
            {
                throw conflict;
            }

            throw;
        }
    }

    public async Task<IReadOnlyList<AcademicYearResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var academicYears = await academicYearRepository.GetAllAsync(cancellationToken);
        var response = academicYears.Select(ToResponse).ToArray();
        return response;
    }

    public async Task<AcademicYearResponse> UpdateAsync(
        long academicYearId,
        UpdateAcademicYearRequest request,
        CancellationToken cancellationToken)
    {
        var proposed = BuildEntity(
            request.Name,
            request.StartDate,
            request.EndDate,
            request.Status);
        proposed.Id = academicYearId;

        var existing = await academicYearRepository.GetTrackedByIdAsync(
            academicYearId,
            cancellationToken);
        if (existing is null)
        {
            throw new NotFoundException("The academic year could not be found.");
        }

        await ValidateConflictsAsync(proposed, academicYearId, cancellationToken);

        var semesters = await academicYearRepository.GetSemestersAsync(
            academicYearId,
            cancellationToken);

        var hasSemesterOutsideRange = semesters.Any(semester =>
            semester.StartDate < proposed.StartDate ||
            semester.EndDate > proposed.EndDate);

        if (hasSemesterOutsideRange)
        {
            throw new ConflictException(SemesterRangeMessage);
        }

        try
        {
            existing.Name = proposed.Name;
            existing.StartDate = proposed.StartDate;
            existing.EndDate = proposed.EndDate;
            existing.Status = proposed.Status;

            await academicYearRepository.SaveChangesAsync(cancellationToken);

            var response = ToResponse(existing);
            return response;
        }
        catch (DbUpdateException exception)
        {
            var conflict = TranslateUniqueConstraintException(exception);
            if (conflict is not null)
            {
                throw conflict;
            }

            throw;
        }
    }

    private async Task ValidateConflictsAsync(
        AcademicYear proposed,
        long? excludedAcademicYearId,
        CancellationToken cancellationToken)
    {
        var academicYears = await academicYearRepository.GetAllAsync(cancellationToken);
        var otherAcademicYears = academicYears.Where(academicYear =>
            academicYear.Id != excludedAcademicYearId);

        if (otherAcademicYears.Any(academicYear =>
                string.Equals(
                    academicYear.Name,
                    proposed.Name,
                    StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException(DuplicateNameMessage);
        }

        if (otherAcademicYears.Any(academicYear =>
                academicYear.StartDate <= proposed.EndDate &&
                academicYear.EndDate >= proposed.StartDate))
        {
            throw new ConflictException(OverlappingRangeMessage);
        }

        if (proposed.Status == AcademicPeriodStatus.ACTIVE &&
            otherAcademicYears.Any(academicYear =>
                academicYear.Status == AcademicPeriodStatus.ACTIVE))
        {
            throw new ConflictException(ActiveYearMessage);
        }
    }

    private static AcademicYear BuildEntity(
        string? name,
        DateOnly? startDate,
        DateOnly? endDate,
        AcademicPeriodStatus? status)
    {
        var normalizedName = name?.Trim() ?? string.Empty;
        if (normalizedName.Length is < 1 or > 200)
        {
            throw new BadRequestException(
                "Academic-year name must contain between 1 and 200 characters after trimming.");
        }

        if (!startDate.HasValue || !endDate.HasValue || !status.HasValue)
        {
            throw new BadRequestException(
                "Name, startDate, endDate, and status are required.");
        }

        if (startDate.Value > endDate.Value)
        {
            throw new BadRequestException(
                "Academic-year startDate must be on or before endDate.");
        }

        if (!Enum.IsDefined(status.Value))
        {
            throw new BadRequestException(
                "Academic-year status must be ACTIVE or COMPLETED.");
        }

        return new AcademicYear
        {
            Name = normalizedName,
            StartDate = startDate.Value,
            EndDate = endDate.Value,
            Status = status.Value
        };
    }

    private static ConflictException? TranslateUniqueConstraintException(
        DbUpdateException exception)
    {
        if (exception.GetBaseException() is not SqlException sqlException ||
            sqlException.Number is not (2601 or 2627))
        {
            return null;
        }

        if (sqlException.Message.Contains(
                "UX_ACADEMIC_YEARS_active_not_deleted",
                StringComparison.OrdinalIgnoreCase))
        {
            return new ConflictException(ActiveYearMessage, exception);
        }

        if (sqlException.Message.Contains(
                "UX_ACADEMIC_YEARS_name_not_deleted",
                StringComparison.OrdinalIgnoreCase))
        {
            return new ConflictException(DuplicateNameMessage, exception);
        }

        return new ConflictException(
            "The academic year conflicts with an existing record.",
            exception);
    }

    private static AcademicYearResponse ToResponse(AcademicYear entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = entity.Status,
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
