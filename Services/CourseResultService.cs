using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Data.Entities.Enums;
using Maiven_Portal_Managment.Repository;
using Maiven_Portal_Managment.Exceptions;

namespace Maiven_Portal_Managment.Services;

public sealed class CourseResultService(CourseResultRepository courseResultRepository)
{
    public async Task FinalizeGradesAsync(
    long sectionId,
    long teacherId,
    CancellationToken cancellationToken)
    {
        var section = await courseResultRepository.GetCourseSectionByIdAsync(sectionId, cancellationToken);

        if (section == null)
            throw new NotFoundException($"Course section {sectionId} does not exist.");

        if (section.TeacherUserRoleId != teacherId)
            throw new UnauthorizedException("You do not have permission to access this section.");

        if (section.Status != CourseSectionStatus.OPEN)
            throw new BadRequestException($"Course section must be OPEN. Current status: {section.Status}.");

        var enrollments = await courseResultRepository.GetEnrollmentsWithScoresAndGradeComponentsAsync(sectionId, cancellationToken);
        var now = DateTime.UtcNow;

        foreach (var enrollment in enrollments)
        {
            decimal finalScore = 0;
            foreach (var score in enrollment.StudentScores)
            {

                finalScore += (score.Score ?? 0m) * (score.Component.Weight / 100m);
            }

            var result = enrollment.CourseResult;
            if (result == null)
            {
                result = new CourseResult
                {
                    EnrollmentId = enrollment.Id,
                    CreatedAt = now
                };
                enrollment.CourseResult = result;
            }

            result.FinalScore = Math.Round(finalScore, 2);
            result.LetterGrade = CalculateLetterGrade(result.FinalScore.Value);
            result.GradePoint = CalculateGradePoint(result.FinalScore.Value);
            result.ResultStatus = CalculateStatus(result.FinalScore.Value);
            result.UpdatedAt = now;
        }

        await courseResultRepository.SaveChangesAsync(cancellationToken);
    }

    private static string CalculateLetterGrade(decimal score) => score switch
    {
        >= 8.5m => "A",
        >= 8.0m => "B+",
        >= 7.0m => "B",
        >= 6.5m => "C+",
        >= 5.5m => "C",
        >= 5.0m => "D+",
        >= 4.0m => "D",
        _ => "F"
    };

    private static ResultStatus CalculateStatus(decimal score)
    {
        return score >= 5.0m ? ResultStatus.PASS : ResultStatus.FAIL;
    }
    private static decimal CalculateGradePoint(decimal finalScore)
    {
        return finalScore / 10m * 4m;
    }
}