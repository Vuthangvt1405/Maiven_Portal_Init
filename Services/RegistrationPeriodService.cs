using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Data.Entities.Enums;
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Services;

public sealed class RegistrationPeriodService(
    RegistrationPeriodRepository registrationPeriodRepository,
    SemesterRepository semesterRepository)
{
    public async Task<RegistrationPeriodResponse> CreateAsync(
        CreateRegistrationPeriodRequest request,
        long currentUserId,
        CancellationToken cancellationToken)
    {
        var semester = await semesterRepository.GetByIdAsync(
            request.SemesterId, 
            cancellationToken);

        if (semester is null)
        {
            throw new NotFoundException("The semester could not be found.");
        }

        var entity = BuildEntity(
            request.SemesterId,
            request.StartAt,
            request.EndAt,
            request.Status,
            currentUserId);

        try
        {
            var created = await registrationPeriodRepository.AddAsync(entity, cancellationToken);
            return ToResponse(created);
        }
        catch (DbUpdateException exception)
        {
            throw new BadRequestException("An error occurred while saving the registration period to the database.", exception);
        }
    }

    public async Task<IReadOnlyList<RegistrationPeriodResponse>> GetAllAsync(
        RegistrationPeriodQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var items = await registrationPeriodRepository.GetAllAsync(
            parameters.SemesterId,
            parameters.Status,
            cancellationToken);

        return items.Select(ToResponse).ToList();
    }

    public async Task<RegistrationPeriodResponse> UpdateAsync(
        long id,
        UpdateRegistrationPeriodRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await registrationPeriodRepository.GetTrackedByIdAsync(id, cancellationToken);
        
        if (existing is null)
        {
            throw new NotFoundException("The registration period could not be found.");
        }

        ValidateUpdateRequest(request.StartAt, request.EndAt, request.Status);

        existing.StartAt = request.StartAt;
        existing.EndAt = request.EndAt;
        existing.Status = request.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        try
        {
            await registrationPeriodRepository.SaveChangesAsync(cancellationToken);
            return ToResponse(existing);
        }
        catch (DbUpdateException exception)
        {
            throw new BadRequestException("An error occurred while updating the data (possibly due to data constraints).", exception);
        }
    }

    private static RegistrationPeriod BuildEntity(
        long semesterId,
        DateTime startAt,
        DateTime endAt,
        RegistrationPeriodStatus status,
        long currentUserId)
    {
        if (startAt >= endAt)
        {
            throw new BadRequestException("Start time must be before end time.");
        }

        if (!Enum.IsDefined(status))
        {
            throw new BadRequestException("Registration period status is invalid.");
        }

        return new RegistrationPeriod
        {
            SemesterId = semesterId,
            StartAt = startAt,
            EndAt = endAt,
            Status = status,
            CreatedById = currentUserId,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static void ValidateUpdateRequest(
        DateTime startAt,
        DateTime endAt,
        RegistrationPeriodStatus status)
    {
        if (startAt >= endAt)
        {
            throw new BadRequestException("Start time must be before end time.");
        }

        if (!Enum.IsDefined(status))
        {
            throw new BadRequestException("Registration period status is invalid.");
        }
    }

    private static RegistrationPeriodResponse ToResponse(RegistrationPeriod entity) => new()
    {
        Id = entity.Id,
        SemesterId = entity.SemesterId,
        StartAt = AsUtc(entity.StartAt),
        EndAt = AsUtc(entity.EndAt),
        Status = entity.Status,
        CreatedById = entity.CreatedById,
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
