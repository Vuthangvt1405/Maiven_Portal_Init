using Maiven_Portal_Managment.Data.Entities.Enums;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Repository;

namespace Maiven_Portal_Managment.Services;

public sealed class ChangeCourseSectionService(CourseSectionRepository courseSectionRepository)
{
    public async Task<ChangeCourseSectionResponse> ChangeCourseSectionCompleteAsync(
        long sectionId,
        long teacherUserRoleId,
        CancellationToken cancellationToken)
    {
        var section = await courseSectionRepository.GetTrackedByIdAsync(sectionId, cancellationToken);

        if (section is null)
        {
            throw new NotFoundException("Course section not found.");
        }

        if (section.TeacherUserRoleId != teacherUserRoleId)
        {
            throw new UnauthorizedException("You are not the teacher for this course section.");
        }

        if (section.Status == CourseSectionStatus.COMPLETED)
        {
            throw new ConflictException("This course section has already been completed.");
        }

        if (section.Status != CourseSectionStatus.OPEN)
        {
            throw new ConflictException("Only an OPEN course section can be marked as completed.");
        }

        section.Status = CourseSectionStatus.COMPLETED;
        section.UpdatedAt = DateTime.UtcNow;

        await courseSectionRepository.SaveChangesAsync(cancellationToken);

        return new ChangeCourseSectionResponse
        {
            SectionId = section.Id,
            SectionCode = section.SectionCode,
            Status = section.Status.ToString(),
            CompletedAt = section.UpdatedAt
        };
    }
}