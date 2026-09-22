using Maiven_Portal_Managment.Data;
using Maiven_Portal_Managment.Data.Entities;
using Maiven_Portal_Managment.Data.Entities.Enums;
using Maiven_Portal_Managment.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MockDataSeederTool;

public sealed class MockDataSeeder(AppDbContext dbContext)
{
    private const string MockPassword = "MockPassword123!";
    private const string MockAcademicYearName = "[MOCK] 2026-2027";
    private readonly PasswordHasher<User> _passwordHasher = new();
    private readonly MockSeedSummary _summary = new();

    public async Task<MockSeedSummary> SeedAsync(CancellationToken cancellationToken)
    {
        var roles = await LoadRequiredRolesAsync(cancellationToken);
        var admin = await LoadRequiredAdminAsync(roles[SystemRoles.Admin.Code], cancellationToken);

        var teachers = await UpsertUsersAsync(
            BuildTeacherSeeds(),
            roles[SystemRoles.Teacher.Code],
            cancellationToken);
        var students = await UpsertUsersAsync(
            BuildStudentSeeds(),
            roles[SystemRoles.Student.Code],
            cancellationToken);

        var academicYear = await UpsertAcademicYearAsync(cancellationToken);
        var semesters = await UpsertSemestersAsync(academicYear, cancellationToken);
        var courses = await UpsertCoursesAsync(cancellationToken);
        var sections = await UpsertSectionsAsync(
            courses,
            semesters,
            teachers,
            cancellationToken);

        await UpsertRegistrationPeriodsAsync(semesters, admin, cancellationToken);
        await UpsertAnnouncementsAsync(sections, teachers, admin, cancellationToken);

        var enrollments = await UpsertEnrollmentsAsync(students, sections, cancellationToken);
        var components = await UpsertGradeComponentsAsync(sections, cancellationToken);
        var scores = await UpsertStudentScoresAsync(
            enrollments,
            components,
            sections,
            teachers,
            cancellationToken);
        await UpsertCourseResultsAsync(enrollments, scores, components, cancellationToken);

        return _summary;
    }

    private async Task<Dictionary<string, Role>> LoadRequiredRolesAsync(
        CancellationToken cancellationToken)
    {
        var requiredCodes = new[]
        {
            SystemRoles.Admin.Code,
            SystemRoles.Teacher.Code,
            SystemRoles.Student.Code
        };

        var roles = await dbContext.Roles
            .IgnoreQueryFilters()
            .Where(role => requiredCodes.Contains(role.Code))
            .ToListAsync(cancellationToken);

        foreach (var code in requiredCodes)
        {
            var role = roles.SingleOrDefault(candidate => candidate.Code == code);
            if (role is null || role.IsDeleted)
            {
                throw new InvalidOperationException(
                    $"The required active role '{code}' is not configured.");
            }
        }

        return roles.ToDictionary(role => role.Code, StringComparer.Ordinal);
    }

    private async Task<User> LoadRequiredAdminAsync(
        Role adminRole,
        CancellationToken cancellationToken)
    {
        var admin = await dbContext.Users
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                user => user.Email == "admin@example.com",
                cancellationToken)
            ?? throw new InvalidOperationException(
                "The migration-seeded administrator is missing.");

        if (admin.IsDeleted)
        {
            throw new InvalidOperationException(
                "The migration-seeded administrator is soft-deleted.");
        }

        var hasAdminRole = await dbContext.UserRoles
            .IgnoreQueryFilters()
            .AnyAsync(
                assignment => assignment.UserId == admin.Id &&
                              assignment.RoleId == adminRole.Id &&
                              !assignment.IsDeleted,
                cancellationToken);
        if (!hasAdminRole)
        {
            throw new InvalidOperationException(
                "The migration-seeded administrator has no active ADMIN role.");
        }

        return admin;
    }

    private async Task<IReadOnlyList<SeededUser>> UpsertUsersAsync(
        IReadOnlyList<MockUserSeed> seeds,
        Role expectedRole,
        CancellationToken cancellationToken)
    {
        var seededUsers = new List<SeededUser>(seeds.Count);

        foreach (var seed in seeds)
        {
            var user = await dbContext.Users
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(candidate => candidate.Email == seed.Email, cancellationToken);

            if (user is null)
            {
                user = new User { Email = seed.Email };
                dbContext.Users.Add(user);
                _summary.UsersAdded++;
            }
            else
            {
                _summary.UsersUpdated++;
            }

            user.PasswordHash = _passwordHasher.HashPassword(
                new User { Email = seed.Email },
                MockPassword);
            user.FullName = seed.FullName;
            user.DateOfBirth = seed.DateOfBirth;
            user.Gender = seed.Gender;
            user.Phone = seed.Phone;
            user.Address = seed.Address;
            user.AvatarUrl = null;
            user.IsDeleted = false;
            seededUsers.Add(new SeededUser(user, null!));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        for (var index = 0; index < seededUsers.Count; index++)
        {
            var user = seededUsers[index].User;
            var assignments = await dbContext.UserRoles
                .IgnoreQueryFilters()
                .Where(assignment => assignment.UserId == user.Id)
                .OrderBy(assignment => assignment.Id)
                .ToListAsync(cancellationToken);
            var activeAssignments = assignments.Where(assignment => !assignment.IsDeleted).ToArray();

            if (activeAssignments.Length > 1 ||
                activeAssignments is [{ RoleId: var activeRoleId }] && activeRoleId != expectedRole.Id)
            {
                throw new InvalidOperationException(
                    $"Mock user '{user.Email}' has an incompatible active role assignment.");
            }

            var assignment = activeAssignments.SingleOrDefault();
            if (assignment is null)
            {
                assignment = assignments.FirstOrDefault(candidate => candidate.RoleId == expectedRole.Id);
                if (assignment is null)
                {
                    assignment = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = expectedRole.Id
                    };
                    dbContext.UserRoles.Add(assignment);
                    _summary.UserRolesAdded++;
                }
                else
                {
                    assignment.IsDeleted = false;
                    _summary.UserRolesUpdated++;
                }
            }
            else
            {
                _summary.UserRolesUpdated++;
            }

            seededUsers[index] = new SeededUser(user, assignment);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return seededUsers;
    }

    private async Task<AcademicYear> UpsertAcademicYearAsync(CancellationToken cancellationToken)
    {
        var academicYear = await dbContext.AcademicYears
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                candidate => candidate.Name == MockAcademicYearName,
                cancellationToken);

        if (academicYear is null)
        {
            academicYear = new AcademicYear { Name = MockAcademicYearName };
            dbContext.AcademicYears.Add(academicYear);
            _summary.AcademicYearsAdded++;
        }
        else
        {
            _summary.AcademicYearsUpdated++;
        }

        academicYear.StartDate = new DateOnly(2026, 9, 1);
        academicYear.EndDate = new DateOnly(2027, 6, 30);
        academicYear.Status = AcademicPeriodStatus.COMPLETED;
        academicYear.IsDeleted = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return academicYear;
    }

    private async Task<IReadOnlyList<Semester>> UpsertSemestersAsync(
        AcademicYear academicYear,
        CancellationToken cancellationToken)
    {
        var seeds = new[]
        {
            new SemesterSeed(
                "[MOCK] Semester 1",
                new DateOnly(2026, 9, 1),
                new DateOnly(2027, 1, 15)),
            new SemesterSeed(
                "[MOCK] Semester 2",
                new DateOnly(2027, 2, 1),
                new DateOnly(2027, 6, 30))
        };
        var semesters = new List<Semester>(seeds.Length);

        foreach (var seed in seeds)
        {
            var semester = await dbContext.Semesters
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    candidate => candidate.AcademicYearId == academicYear.Id &&
                                 candidate.Name == seed.Name,
                    cancellationToken);
            if (semester is null)
            {
                semester = new Semester
                {
                    AcademicYearId = academicYear.Id,
                    Name = seed.Name
                };
                dbContext.Semesters.Add(semester);
                _summary.SemestersAdded++;
            }
            else
            {
                _summary.SemestersUpdated++;
            }

            semester.StartDate = seed.StartDate;
            semester.EndDate = seed.EndDate;
            semester.IsDeleted = false;
            semesters.Add(semester);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return semesters;
    }

    private async Task<IReadOnlyList<Course>> UpsertCoursesAsync(
        CancellationToken cancellationToken)
    {
        var seeds = new[]
        {
            new CourseSeed("MOCK101", "Introduction to Programming", 3),
            new CourseSeed("MOCK102", "Data Structures", 4),
            new CourseSeed("MOCK103", "Database Systems", 3),
            new CourseSeed("MOCK104", "Web Development", 3),
            new CourseSeed("MOCK105", "Cloud Computing", 4)
        };
        var courses = new List<Course>(seeds.Length);

        foreach (var seed in seeds)
        {
            var course = await dbContext.Courses
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    candidate => candidate.CourseCode == seed.Code,
                    cancellationToken);
            if (course is null)
            {
                course = new Course { CourseCode = seed.Code };
                dbContext.Courses.Add(course);
                _summary.CoursesAdded++;
            }
            else
            {
                _summary.CoursesUpdated++;
            }

            course.CourseName = seed.Name;
            course.Credits = seed.Credits;
            course.Description = $"[MOCK] Development data for {seed.Name}.";
            course.Status = ActiveStatus.ACTIVE;
            course.IsDeleted = false;
            courses.Add(course);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return courses;
    }

    private async Task<IReadOnlyList<CourseSection>> UpsertSectionsAsync(
        IReadOnlyList<Course> courses,
        IReadOnlyList<Semester> semesters,
        IReadOnlyList<SeededUser> teachers,
        CancellationToken cancellationToken)
    {
        var sections = new List<CourseSection>(courses.Count * semesters.Count);
        var weekdays = new[]
        {
            WeekDay.MONDAY,
            WeekDay.TUESDAY,
            WeekDay.WEDNESDAY,
            WeekDay.THURSDAY,
            WeekDay.FRIDAY
        };

        for (var semesterIndex = 0; semesterIndex < semesters.Count; semesterIndex++)
        {
            var semester = semesters[semesterIndex];
            for (var courseIndex = 0; courseIndex < courses.Count; courseIndex++)
            {
                var course = courses[courseIndex];
                var sectionCode = $"{course.CourseCode}-{semesterIndex + 1:D2}";
                var section = await dbContext.CourseSections
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(
                        candidate => candidate.SemesterId == semester.Id &&
                                     candidate.SectionCode == sectionCode,
                        cancellationToken);
                if (section is null)
                {
                    section = new CourseSection
                    {
                        SemesterId = semester.Id,
                        SectionCode = sectionCode
                    };
                    dbContext.CourseSections.Add(section);
                    _summary.SectionsAdded++;
                }
                else
                {
                    _summary.SectionsUpdated++;
                }

                var teacher = teachers[(courseIndex + semesterIndex) % teachers.Count];
                var startHour = 8 + (courseIndex % 3) * 3;
                section.CourseId = course.Id;
                section.TeacherUserRoleId = teacher.Role.Id;
                section.Capacity = 30;
                section.DayOfWeek = weekdays[courseIndex];
                section.StartTime = new TimeOnly(startHour, 0);
                section.EndTime = new TimeOnly(startHour + 2, 0);
                section.StartDate = semester.StartDate;
                section.EndDate = semester.EndDate;
                section.Status = CourseSectionStatus.COMPLETED;
                section.IsDeleted = false;
                sections.Add(section);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return sections;
    }

    private async Task UpsertRegistrationPeriodsAsync(
        IReadOnlyList<Semester> semesters,
        User admin,
        CancellationToken cancellationToken)
    {
        foreach (var semester in semesters)
        {
            var startAt = semester.StartDate.ToDateTime(new TimeOnly(0, 0), DateTimeKind.Utc)
                .AddDays(-21);
            var endAt = startAt.AddDays(10);
            var period = await dbContext.RegistrationPeriods
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    candidate => candidate.SemesterId == semester.Id &&
                                 candidate.CreatedById == admin.Id &&
                                 candidate.StartAt == startAt,
                    cancellationToken);
            if (period is null)
            {
                period = new RegistrationPeriod
                {
                    SemesterId = semester.Id,
                    CreatedById = admin.Id,
                    StartAt = startAt
                };
                dbContext.RegistrationPeriods.Add(period);
                _summary.RegistrationPeriodsAdded++;
            }
            else
            {
                _summary.RegistrationPeriodsUpdated++;
            }

            period.EndAt = endAt;
            period.Status = RegistrationPeriodStatus.CLOSED;
            period.IsDeleted = false;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertAnnouncementsAsync(
        IReadOnlyList<CourseSection> sections,
        IReadOnlyList<SeededUser> teachers,
        User admin,
        CancellationToken cancellationToken)
    {
        await UpsertAnnouncementAsync(
            "[MOCK] Academic year information",
            "This is a global development announcement generated by the mock-data seeder.",
            admin.Id,
            null,
            cancellationToken);

        foreach (var section in sections)
        {
            var teacher = teachers.Single(candidate => candidate.Role.Id == section.TeacherUserRoleId);
            await UpsertAnnouncementAsync(
                $"[MOCK] Welcome to {section.SectionCode}",
                $"Welcome to section {section.SectionCode}. This announcement contains mock development data.",
                teacher.User.Id,
                section.Id,
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertAnnouncementAsync(
        string title,
        string content,
        long createdById,
        long? sectionId,
        CancellationToken cancellationToken)
    {
        var announcement = await dbContext.Announcements
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                candidate => candidate.Title == title &&
                             candidate.CreatedById == createdById &&
                             candidate.SectionId == sectionId,
                cancellationToken);
        if (announcement is null)
        {
            announcement = new Announcement
            {
                Title = title,
                CreatedById = createdById,
                SectionId = sectionId
            };
            dbContext.Announcements.Add(announcement);
            _summary.AnnouncementsAdded++;
        }
        else
        {
            _summary.AnnouncementsUpdated++;
        }

        announcement.Content = content;
        announcement.IsDeleted = false;
    }

    private async Task<IReadOnlyList<SeededEnrollment>> UpsertEnrollmentsAsync(
        IReadOnlyList<SeededUser> students,
        IReadOnlyList<CourseSection> sections,
        CancellationToken cancellationToken)
    {
        var enrollments = new List<SeededEnrollment>(students.Count * 3);

        for (var studentIndex = 0; studentIndex < students.Count; studentIndex++)
        {
            var student = students[studentIndex];
            for (var choiceIndex = 0; choiceIndex < 3; choiceIndex++)
            {
                var sectionIndex = (studentIndex + choiceIndex * 3) % sections.Count;
                var section = sections[sectionIndex];
                var enrollment = await dbContext.Enrollments
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(
                        candidate => candidate.StudentUserRoleId == student.Role.Id &&
                                     candidate.SectionId == section.Id,
                        cancellationToken);
                if (enrollment is null)
                {
                    enrollment = new Enrollment
                    {
                        StudentUserRoleId = student.Role.Id,
                        SectionId = section.Id
                    };
                    dbContext.Enrollments.Add(enrollment);
                    _summary.EnrollmentsAdded++;
                }
                else
                {
                    _summary.EnrollmentsUpdated++;
                }

                enrollment.IsDeleted = false;
                enrollments.Add(new SeededEnrollment(enrollment, studentIndex, sectionIndex));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return enrollments;
    }

    private async Task<IReadOnlyDictionary<long, IReadOnlyList<GradeComponent>>>
        UpsertGradeComponentsAsync(
            IReadOnlyList<CourseSection> sections,
            CancellationToken cancellationToken)
    {
        var componentSeeds = new[]
        {
            new GradeComponentSeed("Attendance", 10m),
            new GradeComponentSeed("Midterm", 30m),
            new GradeComponentSeed("Final", 60m)
        };
        var result = new Dictionary<long, IReadOnlyList<GradeComponent>>();

        foreach (var section in sections)
        {
            var sectionComponents = new List<GradeComponent>(componentSeeds.Length);
            foreach (var seed in componentSeeds)
            {
                var component = await dbContext.GradeComponents
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(
                        candidate => candidate.SectionId == section.Id &&
                                     candidate.Name == seed.Name,
                        cancellationToken);
                if (component is null)
                {
                    component = new GradeComponent
                    {
                        SectionId = section.Id,
                        Name = seed.Name
                    };
                    dbContext.GradeComponents.Add(component);
                    _summary.GradeComponentsAdded++;
                }
                else
                {
                    _summary.GradeComponentsUpdated++;
                }

                component.Weight = seed.Weight;
                component.IsDeleted = false;
                sectionComponents.Add(component);
            }

            result[section.Id] = sectionComponents;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task<IReadOnlyDictionary<long, IReadOnlyList<StudentScore>>>
        UpsertStudentScoresAsync(
            IReadOnlyList<SeededEnrollment> enrollments,
            IReadOnlyDictionary<long, IReadOnlyList<GradeComponent>> components,
            IReadOnlyList<CourseSection> sections,
            IReadOnlyList<SeededUser> teachers,
            CancellationToken cancellationToken)
    {
        var result = new Dictionary<long, IReadOnlyList<StudentScore>>();

        foreach (var seededEnrollment in enrollments)
        {
            var enrollment = seededEnrollment.Enrollment;
            var section = sections[seededEnrollment.SectionIndex];
            var teacher = teachers.Single(candidate => candidate.Role.Id == section.TeacherUserRoleId);
            var enrollmentScores = new List<StudentScore>(3);
            var sectionComponents = components[section.Id];

            for (var componentIndex = 0; componentIndex < sectionComponents.Count; componentIndex++)
            {
                var component = sectionComponents[componentIndex];
                var score = await dbContext.StudentScores
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(
                        candidate => candidate.EnrollmentId == enrollment.Id &&
                                     candidate.ComponentId == component.Id,
                        cancellationToken);
                if (score is null)
                {
                    score = new StudentScore
                    {
                        EnrollmentId = enrollment.Id,
                        ComponentId = component.Id
                    };
                    dbContext.StudentScores.Add(score);
                    _summary.StudentScoresAdded++;
                }
                else
                {
                    _summary.StudentScoresUpdated++;
                }

                score.Score = 55m +
                    (seededEnrollment.StudentIndex * 7 +
                     seededEnrollment.SectionIndex * 3 +
                     componentIndex * 11) % 41;
                score.UpdatedById = teacher.User.Id;
                score.IsDeleted = false;
                enrollmentScores.Add(score);
            }

            result[enrollment.Id] = enrollmentScores;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task UpsertCourseResultsAsync(
        IReadOnlyList<SeededEnrollment> enrollments,
        IReadOnlyDictionary<long, IReadOnlyList<StudentScore>> scores,
        IReadOnlyDictionary<long, IReadOnlyList<GradeComponent>> components,
        CancellationToken cancellationToken)
    {
        foreach (var seededEnrollment in enrollments)
        {
            var enrollment = seededEnrollment.Enrollment;
            var enrollmentScores = scores[enrollment.Id];
            var componentById = components[enrollment.SectionId]
                .ToDictionary(component => component.Id);
            var finalScore = decimal.Round(
                enrollmentScores.Sum(score =>
                    score.Score!.Value * componentById[score.ComponentId].Weight / 100m),
                2,
                MidpointRounding.AwayFromZero);
            var result = await dbContext.CourseResults
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    candidate => candidate.EnrollmentId == enrollment.Id,
                    cancellationToken);
            if (result is null)
            {
                result = new CourseResult { EnrollmentId = enrollment.Id };
                dbContext.CourseResults.Add(result);
                _summary.CourseResultsAdded++;
            }
            else
            {
                _summary.CourseResultsUpdated++;
            }

            var grade = GetGrade(finalScore);
            result.FinalScore = finalScore;
            result.LetterGrade = grade.Letter;
            result.GradePoint = grade.Point;
            result.ResultStatus = finalScore >= 60m ? ResultStatus.PASS : ResultStatus.FAIL;
            result.IsDeleted = false;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static (string Letter, decimal Point) GetGrade(decimal score) => score switch
    {
        >= 90m => ("A", 4.00m),
        >= 80m => ("B", 3.00m),
        >= 70m => ("C", 2.00m),
        >= 60m => ("D", 1.00m),
        _ => ("F", 0.00m)
    };

    private static IReadOnlyList<MockUserSeed> BuildTeacherSeeds() =>
        Enumerable.Range(1, 3)
            .Select(index => new MockUserSeed(
                $"mock.teacher{index:D2}@maiven.local",
                $"Mock Teacher {index:D2}",
                new DateOnly(1985 + index, index + 1, 10 + index),
                index % 2 == 0 ? Gender.FEMALE : Gender.MALE,
                $"090100{index:D4}",
                $"[MOCK] Faculty Office {index:D2}"))
            .ToArray();

    private static IReadOnlyList<MockUserSeed> BuildStudentSeeds() =>
        Enumerable.Range(1, 15)
            .Select(index => new MockUserSeed(
                $"mock.student{index:D2}@maiven.local",
                $"Mock Student {index:D2}",
                new DateOnly(2003 + index % 3, index % 12 + 1, index % 20 + 1),
                (Gender)(index % 3),
                $"091200{index:D4}",
                $"[MOCK] Student Address {index:D2}"))
            .ToArray();

    private sealed record MockUserSeed(
        string Email,
        string FullName,
        DateOnly DateOfBirth,
        Gender Gender,
        string Phone,
        string Address);

    private sealed record SeededUser(User User, UserRole Role);
    private sealed record SeededEnrollment(
        Enrollment Enrollment,
        int StudentIndex,
        int SectionIndex);
    private sealed record SemesterSeed(string Name, DateOnly StartDate, DateOnly EndDate);
    private sealed record CourseSeed(string Code, string Name, int Credits);
    private sealed record GradeComponentSeed(string Name, decimal Weight);
}

public sealed class MockSeedSummary
{
    public int UsersAdded { get; set; }
    public int UsersUpdated { get; set; }
    public int UserRolesAdded { get; set; }
    public int UserRolesUpdated { get; set; }
    public int AcademicYearsAdded { get; set; }
    public int AcademicYearsUpdated { get; set; }
    public int SemestersAdded { get; set; }
    public int SemestersUpdated { get; set; }
    public int CoursesAdded { get; set; }
    public int CoursesUpdated { get; set; }
    public int SectionsAdded { get; set; }
    public int SectionsUpdated { get; set; }
    public int RegistrationPeriodsAdded { get; set; }
    public int RegistrationPeriodsUpdated { get; set; }
    public int AnnouncementsAdded { get; set; }
    public int AnnouncementsUpdated { get; set; }
    public int EnrollmentsAdded { get; set; }
    public int EnrollmentsUpdated { get; set; }
    public int GradeComponentsAdded { get; set; }
    public int GradeComponentsUpdated { get; set; }
    public int StudentScoresAdded { get; set; }
    public int StudentScoresUpdated { get; set; }
    public int CourseResultsAdded { get; set; }
    public int CourseResultsUpdated { get; set; }

    public override string ToString() =>
        $"Users +{UsersAdded}/~{UsersUpdated}, roles +{UserRolesAdded}/~{UserRolesUpdated}, " +
        $"academic years +{AcademicYearsAdded}/~{AcademicYearsUpdated}, " +
        $"semesters +{SemestersAdded}/~{SemestersUpdated}, courses +{CoursesAdded}/~{CoursesUpdated}, " +
        $"sections +{SectionsAdded}/~{SectionsUpdated}, " +
        $"registration periods +{RegistrationPeriodsAdded}/~{RegistrationPeriodsUpdated}, " +
        $"announcements +{AnnouncementsAdded}/~{AnnouncementsUpdated}, " +
        $"enrollments +{EnrollmentsAdded}/~{EnrollmentsUpdated}, " +
        $"grade components +{GradeComponentsAdded}/~{GradeComponentsUpdated}, " +
        $"scores +{StudentScoresAdded}/~{StudentScoresUpdated}, " +
        $"results +{CourseResultsAdded}/~{CourseResultsUpdated}.";
}
