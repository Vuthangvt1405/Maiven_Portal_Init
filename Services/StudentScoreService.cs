using Maiven_Portal_Managment.Common;
using Maiven_Portal_Managment.Dtos.request;
using Maiven_Portal_Managment.Dtos.response;
using Maiven_Portal_Managment.Exceptions;
using Maiven_Portal_Managment.Repository;
using Maiven_Portal_Managment.Services.Security;

namespace Maiven_Portal_Managment.Services;

public sealed class StudentScoreService(
    StudentScoreRepository studentScoreRepository,
    CurrentUserContext currentUserContext)
{
    public async Task<StudentScoreResponse> UpdateAsync(
        long studentScoreId,
        UpdateStudentScoreRequest request,
        CancellationToken cancellationToken)
    {
        if (!currentUserContext.IsAuthenticated ||
            currentUserContext.UserId is not long teacherUserId ||
            currentUserContext.RoleUserId is not long teacherUserRoleId)
        {
            throw new UnauthorizedException("An authenticated teacher is required.");
        }

        var studentScore = await studentScoreRepository.GetByIdWithSectionAsync(
            studentScoreId,
            cancellationToken);

        if (studentScore is null)
        {
            throw new NotFoundException("The student score could not be found.");
        }

        var section = studentScore.Enrollment.Section;
        if (section.TeacherUserRoleId != teacherUserRoleId)
        {
            throw new UnauthorizedException("You are not assigned to this course section.");
        }

        var updatedScore = await studentScoreRepository.UpdateScoreAsync(
            studentScore,
            request.Score,
            teacherUserId,
            cancellationToken);

        return new StudentScoreResponse(
            updatedScore.Id,
            updatedScore.EnrollmentId,
            updatedScore.ComponentId,
            updatedScore.Score,
            updatedScore.UpdatedById,
            updatedScore.UpdatedAt);
    }
}
