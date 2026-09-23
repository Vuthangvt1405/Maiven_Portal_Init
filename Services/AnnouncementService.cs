using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.Response;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Repository;
using Maiven_Portal_Managment.Services.Security;

namespace Maiven_Portal_Managment.Services;

public sealed class AnnouncementService(
    AnnouncementRepository announcementRepository,
    CurrentUserContext currentUserContext)
{
    public async Task<AnnouncementResponse> CreateAsync(CreateAnnouncementRequest request, CancellationToken cancellationToken)
    {
        var entity = BuildEntity(GetCurrentUserId(), request.SectionId, request.Title, request.Content);
        var created = await announcementRepository.AddAsync(entity, cancellationToken);
        var announcement = await announcementRepository.GetByIdAsync(created.Id, cancellationToken)
            ?? throw new NotFoundException("The announcement could not be found after creation.");
        return ToResponse(announcement);
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
        var items = result.Items.Select(ToResponse).ToArray();
        return (items, result.TotalItems);
    }

    public async Task<AnnouncementResponse> GetByIdAsync(long announcementId, CancellationToken cancellationToken)
    {
        var announcement = await announcementRepository.GetByIdAsync(announcementId, cancellationToken)
            ?? throw new NotFoundException("The announcement could not be found.");
        var response = ToResponse(announcement);
        return response;
    }

    public async Task<AnnouncementResponse> UpdateAsync(
        long announcementId,
        UpdateAnnouncementRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await announcementRepository.GetTrackedByIdAsync(announcementId, cancellationToken)
            ?? throw new NotFoundException("The announcement could not be found.");
        EnsureOwnerOrAdmin(existing.CreatedById);

        var validated = BuildEntity(existing.CreatedById, request.SectionId, request.Title, request.Content);
        existing.SectionId = validated.SectionId;
        existing.Title = validated.Title;
        existing.Content = validated.Content;

        await announcementRepository.SaveChangesAsync(cancellationToken);
        var response = ToResponse(existing);
        return response;
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

    private static Announcement BuildEntity(long createdById, long? sectionId, string? title, string? content)
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

        return new Announcement
        {
            CreatedById = createdById,
            SectionId = sectionId,
            Title = normalizedTitle,
            Content = normalizedContent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static AnnouncementResponse ToResponse(Announcement entity) => new()
    {
        Id = entity.Id,
        CreatedById = entity.CreatedById,
        User = ToUserResponse(entity.CreatedBy),
        SectionId = entity.SectionId,
        Title = entity.Title,
        Content = entity.Content,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    private static AuthUserResponse ToUserResponse(User user)
    {
        var userRole = user.UserRoles.FirstOrDefault();

        return new AuthUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Phone = user.Phone,
            Address = user.Address,
            AvatarUrl = user.AvatarUrl,
            Role = userRole?.Role.Code ?? string.Empty,
            RoleUserId = userRole?.Id ?? 0
        };
    }
}