using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentManager.Models;

namespace StudentManager.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<Assignment> Assignments => Set<Assignment>();

    public DbSet<Submission> Submissions => Set<Submission>();

    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureCourse(builder);
        ConfigureEnrollment(builder);
        ConfigureAssignment(builder);
        ConfigureSubmission(builder);
        ConfigureGrade(builder);
    }

    private static void ConfigureCourse(ModelBuilder builder)
    {
        builder.Entity<Course>()
            .HasIndex(course => course.Code)
            .IsUnique();

        builder.Entity<Course>()
            .HasOne(course => course.Teacher)
            .WithMany(user => user.CoursesTaught)
            .HasForeignKey(course => course.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureEnrollment(ModelBuilder builder)
    {
        builder.Entity<Enrollment>()
            .HasIndex(enrollment => new
            {
                enrollment.StudentId,
                enrollment.CourseId
            })
            .IsUnique();

        builder.Entity<Enrollment>()
            .HasOne(enrollment => enrollment.Student)
            .WithMany(user => user.Enrollments)
            .HasForeignKey(enrollment => enrollment.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Enrollment>()
            .HasOne(enrollment => enrollment.Course)
            .WithMany(course => course.Enrollments)
            .HasForeignKey(enrollment => enrollment.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureAssignment(ModelBuilder builder)
    {
        builder.Entity<Assignment>()
            .Property(assignment => assignment.MaxPoints)
            .HasPrecision(10, 2);

        builder.Entity<Assignment>()
            .HasOne(assignment => assignment.Course)
            .WithMany(course => course.Assignments)
            .HasForeignKey(assignment => assignment.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureSubmission(ModelBuilder builder)
    {
        builder.Entity<Submission>()
            .HasIndex(submission => new
            {
                submission.AssignmentId,
                submission.StudentId
            })
            .IsUnique();

        builder.Entity<Submission>()
            .HasOne(submission => submission.Assignment)
            .WithMany(assignment => assignment.Submissions)
            .HasForeignKey(submission => submission.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Submission>()
            .HasOne(submission => submission.Student)
            .WithMany(user => user.Submissions)
            .HasForeignKey(submission => submission.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureGrade(ModelBuilder builder)
    {
        builder.Entity<Grade>()
            .Property(grade => grade.Score)
            .HasPrecision(10, 2);

        builder.Entity<Grade>()
            .HasIndex(grade => grade.SubmissionId)
            .IsUnique();

        builder.Entity<Grade>()
            .HasOne(grade => grade.Submission)
            .WithOne(submission => submission.Grade)
            .HasForeignKey<Grade>(grade => grade.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Grade>()
            .HasOne(grade => grade.GradedBy)
            .WithMany(user => user.GradesGiven)
            .HasForeignKey(grade => grade.GradedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}