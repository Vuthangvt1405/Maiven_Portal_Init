using Maiven_Portal_Managment.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maiven_Portal_Managment.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<FaceCredential> FaceCredentials => Set<FaceCredential>();
    public DbSet<PasswordResetRequest> PasswordResetRequests => Set<PasswordResetRequest>();
    public DbSet<EmailSuffixWhitelistRule> EmailSuffixWhitelistRules => Set<EmailSuffixWhitelistRule>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseSection> CourseSections => Set<CourseSection>();
    public DbSet<RegistrationPeriod> RegistrationPeriods => Set<RegistrationPeriod>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<GradeComponent> GradeComponents => Set<GradeComponent>();
    public DbSet<StudentScore> StudentScores => Set<StudentScore>();
    public DbSet<CourseResult> CourseResults => Set<CourseResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<FaceCredential>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<PasswordResetRequest>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<EmailSuffixWhitelistRule>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<UserRole>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<AcademicYear>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Semester>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Course>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<CourseSection>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<RegistrationPeriod>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Enrollment>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Announcement>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<GradeComponent>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<StudentScore>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<CourseResult>().HasQueryFilter(x => !x.IsDeleted);
    }

    public override int SaveChanges() => SaveChanges(acceptAllChangesOnSuccess: true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PrepareTrackedEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        PrepareTrackedEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void PrepareTrackedEntities()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
            }

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }

        foreach (var entry in ChangeTracker.Entries<StudentScore>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }
}
