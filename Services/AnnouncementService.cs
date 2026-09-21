using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Models;
using Maiven_Portal_Managment.Repository;
using Maiven_Portal_Managment.Services.Security;

namespace Maiven_Portal_Managment.Services;

public sealed class AnnouncementService(
    AnnouncementRepository announcementRepository,
    CurrentUserContext currentUserContext)
{
    public async Task<AnnouncementResponse> CreateAsync(CreateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        var model = BuildModel(GetCurrentUserId(), request.SectionId, request.Title, request.Content);
        return ToResponse(await announcementRepository.AddAsync(model, cancellationToken));
    }

    public async Task<(IReadOnlyList<AnnouncementResponse> Items, int TotalItems)> GetPagedAsync(
        long? sectionId,
        string? title,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await announcementRepository.GetPagedAsync(
            sectionId, title, pageNumber, pageSize, cancellationToken);
        return (result.Items.Select(ToResponse).ToArray(), result.TotalItems);
    }

    public async Task<AnnouncementResponse> GetByIdAsync(long announcementId, CancellationToken cancellationToken)
    {
        var announcement = await announcementRepository.GetByIdAsync(announcementId, cancellationToken)
            ?? throw new NotFoundException("The announcement could not be found.");
        return ToResponse(announcement);
    }

    public async Task<AnnouncementResponse> UpdateAsync(
        long announcementId,
        UpdateAnnouncementRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await announcementRepository.GetByIdAsync(announcementId, cancellationToken)
            ?? throw new NotFoundException("The announcement could not be found.");
        EnsureOwnerOrAdmin(existing.CreatedById);

        var updated = BuildModel(existing.CreatedById, request.SectionId, request.Title, request.Content);
        updated.Id = announcementId;
        var result = await announcementRepository.UpdateAsync(updated, cancellationToken)
            ?? throw new NotFoundException("The announcement could not be found.");
        return ToResponse(result);
    }

    public async Task DeleteAsync(long announcementId, CancellationToken cancellationToken)
    {
        var existing = await announcementRepository.GetByIdAsync(announcementId, cancellationToken)
            ?? throw new NotFoundException("The announcement could not be found.");
        EnsureOwnerOrAdmin(existing.CreatedById);

        if (!await announcementRepository.DeleteAsync(announcementId, cancellationToken))
        {
            throw new NotFoundException("The announcement could not be found.");
        }
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