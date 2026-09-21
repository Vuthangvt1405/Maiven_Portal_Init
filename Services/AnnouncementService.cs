using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Logging;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Repository;
using Maiven_Portal_Managment.Services.Security;

namespace Maiven_Portal_Managment.Services;

public sealed class AnnouncementService(
    AnnouncementRepository announcementRepository,
    CurrentUserContext currentUserContext,
    ActionLogService actionLogService)
{
    public async Task<AnnouncementResponse> CreateAsync(CreateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        using var operation = actionLogService.Begin<AnnouncementService>(
            "Service",
            nameof(CreateAsync),
            ("CourseSectionId", request.SectionId));
        var model = BuildModel(GetCurrentUserId(), request.SectionId, request.Title, request.Content);
        var response = ToResponse(await announcementRepository.AddAsync(model, cancellationToken));
        operation.Complete(("AnnouncementId", response.Id));
        return response;
    }

    public async Task<(IReadOnlyList<AnnouncementResponse> Items, int TotalItems)> GetPagedAsync(
        long? sectionId,
        string? title,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        using var operation = actionLogService.Begin<AnnouncementService>(
            "Service",
            nameof(GetPagedAsync),
            ("CourseSectionId", sectionId),
            ("PageNumber", pageNumber),
            ("PageSize", pageSize));
        var result = await announcementRepository.GetPagedAsync(
            sectionId, title, pageNumber, pageSize, cancellationToken);
        var items = result.Items.Select(ToResponse).ToArray();
        operation.Complete(
            ("ResultCount", items.Length),
            ("TotalItems", result.TotalItems));
        return (items, result.TotalItems);
    }

    public async Task<AnnouncementResponse> GetByIdAsync(long announcementId, CancellationToken cancellationToken)
    {
        using var operation = actionLogService.Begin<AnnouncementService>(
            "Service",
            nameof(GetByIdAsync),
            ("AnnouncementId", announcementId));
        var announcement = await announcementRepository.GetByIdAsync(announcementId, cancellationToken)
            ?? throw new NotFoundException("The announcement could not be found.");
        var response = ToResponse(announcement);
        operation.Complete(("AnnouncementId", response.Id));
        return response;
    }

    public async Task<AnnouncementResponse> UpdateAsync(
        long announcementId,
        UpdateAnnouncementRequest request,
        CancellationToken cancellationToken)
    {
        using var operation = actionLogService.Begin<AnnouncementService>(
            "Service",
            nameof(UpdateAsync),
            ("AnnouncementId", announcementId));
        var existing = await announcementRepository.GetByIdAsync(announcementId, cancellationToken)
            ?? throw new NotFoundException("The announcement could not be found.");
        EnsureOwnerOrAdmin(existing.CreatedById);

        var updated = BuildModel(existing.CreatedById, request.SectionId, request.Title, request.Content);
        updated.Id = announcementId;
        var result = await announcementRepository.UpdateAsync(updated, cancellationToken)
            ?? throw new NotFoundException("The announcement could not be found.");
        var response = ToResponse(result);
        operation.Complete(("AnnouncementId", response.Id));
        return response;
    }

    public async Task DeleteAsync(long announcementId, CancellationToken cancellationToken)
    {
        using var operation = actionLogService.Begin<AnnouncementService>(
            "Service",
            nameof(DeleteAsync),
            ("AnnouncementId", announcementId));
        var existing = await announcementRepository.GetByIdAsync(announcementId, cancellationToken)
            ?? throw new NotFoundException("The announcement could not be found.");
        EnsureOwnerOrAdmin(existing.CreatedById);

        if (!await announcementRepository.DeleteAsync(announcementId, cancellationToken))
        {
            throw new NotFoundException("The announcement could not be found.");
        }

        operation.Complete(("AnnouncementId", announcementId));
    }

    private long GetCurrentUserId() => currentUserContext.UserId
        ?? throw new UnauthorizedException("An authenticated user is required.");

    private void EnsureOwnerOrAdmin(long createdById)
    {
        if (GetCurrentUserId() != createdById && currentUserContext.Role != SystemRoles.Admin.Code)
        {
            throw new UnauthorizedException("You are not allowed to modify this announcement.");
        }
    }

    private static AnnouncementModel BuildModel(long createdById, long? sectionId, string? title, string? content)
    {
        var normalizedTitle = title?.Trim() ?? string.Empty;
        var normalizedContent = content?.Trim() ?? string.Empty;

        if (normalizedTitle.Length is < 1 or > 200)
        {
            throw new BadRequestException("Announcement title must contain between 1 and 200 characters.");
        }

        if (normalizedContent.Length < 1)
        {
            throw new BadRequestException("Announcement content is required.");
        }

        return new AnnouncementModel
        {
            CreatedById = createdById,
            SectionId = sectionId,
            Title = normalizedTitle,
            Content = normalizedContent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static AnnouncementResponse ToResponse(AnnouncementModel model) => new()
    {
        Id = model.Id,
        CreatedById = model.CreatedById,
        SectionId = model.SectionId,
        Title = model.Title,
        Content = model.Content,
        CreatedAt = model.CreatedAt,
        UpdatedAt = model.UpdatedAt
    };
}