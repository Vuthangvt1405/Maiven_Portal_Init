using Maiven_Portal_Managment.Dtos.Request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Models.Enums;
using Maiven_Portal_Managment.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Services;

public sealed class AcademicYearService(AcademicYearRepository academicYearRepository)
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
        var model = BuildModel(
            request.Name,
            request.StartDate,
            request.EndDate,
            request.Status);

        await ValidateConflictsAsync(model, null, cancellationToken);

        try
        {
            var created = await academicYearRepository.AddAsync(model, cancellationToken);
            return ToResponse(created);
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
        return academicYears.Select(ToResponse).ToArray();
    }

    public async Task<AcademicYearResponse> UpdateAsync(
        long academicYearId,
        UpdateAcademicYearRequest request,
        CancellationToken cancellationToken)
    {
        var proposed = BuildModel(
            request.Name,
            request.StartDate,
            request.EndDate,
            request.Status);
        proposed.Id = academicYearId;

        var existing = await academicYearRepository.GetByIdAsync(
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
            var updated = await academicYearRepository.UpdateAsync(
                proposed,
                cancellationToken);
            if (updated is null)
            {
                throw new NotFoundException("The academic year could not be found.");
            }

            return ToResponse(updated);
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
        AcademicYearModel proposed,
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

    private static AcademicYearModel BuildModel(
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

        return new AcademicYearModel
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

    private static AcademicYearResponse ToResponse(AcademicYearModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        StartDate = model.StartDate,
        EndDate = model.EndDate,
        Status = model.Status,
        CreatedAt = AsUtc(model.CreatedAt),
        UpdatedAt = AsUtc(model.UpdatedAt)
    };

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
