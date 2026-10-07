using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentManager.Models;
using StudentManager.Models.Enums;

namespace StudentManager.Data;

public static class SeedData
{
    private const string DemoPassword = "StudentDemo123";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var serviceProvider = scope.ServiceProvider;
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

        var teachers = new[]
        {
            new UserSeed("teacher1@school.test", "Avery", "Morgan", null, "Teacher"),
            new UserSeed("teacher2@school.test", "Jordan", "Lee", null, "Teacher"),
            new UserSeed("teacher3@school.test", "Taylor", "Reed", null, "Teacher")
        };

        var students = new[]
        {
            new UserSeed("student1@school.test", "Riley", "Chen", "STU1001", "Student"),
            new UserSeed("student2@school.test", "Casey", "Patel", "STU1002", "Student"),
            new UserSeed("student3@school.test", "Quinn", "Diaz", "STU1003", "Student")
        };

        var teacherUsers = new List<ApplicationUser>();
        foreach (var seed in teachers)
        {
            teacherUsers.Add(await EnsureUserAsync(userManager, seed));
        }

        var studentUsers = new List<ApplicationUser>();
        foreach (var seed in students)
        {
            studentUsers.Add(await EnsureUserAsync(userManager, seed));
        }

        var courseSeeds = new[]
        {
            new CourseSeed("DEMO101", "Foundations of Programming", 0),
            new CourseSeed("DEMO201", "Web Application Development", 1),
            new CourseSeed("DEMO301", "Data and Database Design", 2)
        };

        var courses = new List<Course>();
        foreach (var seed in courseSeeds)
        {
            var course = await dbContext.Courses
                .SingleOrDefaultAsync(item => item.Code == seed.Code);

            if (course is null)
            {
                course = new Course
                {
                    Code = seed.Code,
                    Name = seed.Name,
                    Description = $"Development sample course: {seed.Name}.",
                    TeacherId = teacherUsers[seed.TeacherIndex].Id,
                    IsActive = true
                };
                dbContext.Courses.Add(course);
            }

            courses.Add(course);
        }

        await dbContext.SaveChangesAsync();

        var assignmentSeeds = new[]
        {
            new AssignmentSeed(0, "Variables and Control Flow"),
            new AssignmentSeed(1, "Build a Course Portal"),
            new AssignmentSeed(1, "Display an assignment from a list of assignments"),
            new AssignmentSeed(1, "Display a second assignment from a list of assignments"),
            new AssignmentSeed(2, "Relational Data Model")
        };

        var assignments = new List<Assignment>();
        foreach (var seed in assignmentSeeds)
        {
            var assignment = await dbContext.Assignments
                .SingleOrDefaultAsync(item =>
                    item.CourseId == courses[seed.CourseIndex].CourseId &&
                    item.Title == seed.Title);

            if (assignment is null)
            {
                assignment = new Assignment
                {
                    CourseId = courses[seed.CourseIndex].CourseId,
                    Title = seed.Title,
                    Description = "Complete the sample assignment and submit your work.",
                    DueDate = DateTime.UtcNow.AddDays(14 + seed.CourseIndex * 7),
                    MaxPoints = 100,
                    IsActive = true
                };
                dbContext.Assignments.Add(assignment);
            }

            assignments.Add(assignment);
        }

        await dbContext.SaveChangesAsync();

        for (var index = 0; index < courses.Count; index++)
        {
            var student = studentUsers[index];
            var course = courses[index];
            var enrollmentExists = await dbContext.Enrollments.AnyAsync(item =>
                item.StudentId == student.Id && item.CourseId == course.CourseId);

            if (!enrollmentExists)
            {
                dbContext.Enrollments.Add(new Enrollment
                {
                    StudentId = student.Id,
                    CourseId = course.CourseId,
                    Status = EnrollmentStatus.Active
                });
            }
        }

        await dbContext.SaveChangesAsync();

        var submissions = new List<Submission>();
        for (var index = 0; index < 3; index++)
        {
            var assignment = assignments[index];
            var student = studentUsers[index];
            var submission = await dbContext.Submissions
                .SingleOrDefaultAsync(item =>
                    item.AssignmentId == assignment.AssignmentId &&
                    item.StudentId == student.Id);

            if (submission is null)
            {
                submission = new Submission
                {
                    AssignmentId = assignment.AssignmentId,
                    StudentId = student.Id,
                    Content = $"Sample submission from {student.FirstName} for {assignment.Title}.",
                    IsLate = false
                };
                dbContext.Submissions.Add(submission);
            }

            submissions.Add(submission);
        }

        await dbContext.SaveChangesAsync();

        for (var index = 0; index < submissions.Count; index++)
        {
            var submission = submissions[index];
            var gradeExists = await dbContext.Grades
                .AnyAsync(item => item.SubmissionId == submission.SubmissionId);

            if (!gradeExists)
            {
                dbContext.Grades.Add(new Grade
                {
                    SubmissionId = submission.SubmissionId,
                    Score = 85 + index * 5,
                    Feedback = "Sample feedback: clear work and good attention to detail.",
                    GradedById = teacherUsers[index].Id
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        UserSeed seed)
    {
        var user = await userManager.FindByEmailAsync(seed.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = seed.Email,
                Email = seed.Email,
                EmailConfirmed = true,
                FirstName = seed.FirstName,
                LastName = seed.LastName,
                StudentNumber = seed.StudentNumber
            };

            var createResult = await userManager.CreateAsync(user, DemoPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException(
                    $"Could not create demo user '{seed.Email}': {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, seed.Role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, seed.Role);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException(
                    $"Could not assign role '{seed.Role}' to demo user '{seed.Email}': {errors}");
            }
        }

        return user;
    }

    private sealed record UserSeed(
        string Email,
        string FirstName,
        string LastName,
        string? StudentNumber,
        string Role);

    private sealed record CourseSeed(string Code, string Name, int TeacherIndex);

    private sealed record AssignmentSeed(int CourseIndex, string Title);
}