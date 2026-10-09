using Microsoft.EntityFrameworkCore;
using StudentManager.Data;
using StudentManager.Models;
using StudentManager.Models.Enums;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Student;

namespace StudentManager.Services;

public class SubmissionService : ISubmissionService
{
    private readonly ApplicationDbContext _context;

    public SubmissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StudentAssignmentViewModel>>
        GetAllAssignmentsAsync(string studentId)
    {
        var assignments = await _context.Assignments
            .AsNoTracking()
            .Where(assignment =>
                assignment.IsActive &&
                assignment.Course.IsActive &&
                assignment.Course.Enrollments.Any(enrollment =>
                    enrollment.StudentId == studentId &&
                    enrollment.Status == EnrollmentStatus.Active))
            .Include(assignment => assignment.Course)
            .Include(assignment => assignment.Submissions
                .Where(submission =>
                    submission.StudentId == studentId))
                .ThenInclude(submission => submission.Grade)
            .OrderBy(assignment => assignment.DueDate)
            .ThenBy(assignment => assignment.Course.Code)
            .ThenBy(assignment => assignment.Title)
            .ToListAsync();

        return assignments
            .Select(MapAssignment)
            .ToList();
    }

    public async Task<CourseAssignmentsViewModel?>
        GetCourseAssignmentsAsync(int courseId, string studentId)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Where(item =>
                item.CourseId == courseId &&
                item.IsActive &&
                item.Enrollments.Any(enrollment =>
                    enrollment.StudentId == studentId &&
                    enrollment.Status == EnrollmentStatus.Active))
            .Select(item => new
            {
                item.CourseId,
                item.Code,
                item.Name
            })
            .FirstOrDefaultAsync();

        if (course is null)
        {
            return null;
        }

        var assignments = await _context.Assignments
            .AsNoTracking()
            .Where(assignment =>
                assignment.CourseId == courseId &&
                assignment.IsActive)
            .Include(assignment => assignment.Course)
            .Include(assignment => assignment.Submissions
                .Where(submission =>
                    submission.StudentId == studentId))
                .ThenInclude(submission => submission.Grade)
            .OrderBy(assignment => assignment.DueDate)
            .ToListAsync();

        return new CourseAssignmentsViewModel
        {
            CourseId = course.CourseId,
            CourseCode = course.Code,
            CourseName = course.Name,
            Assignments = assignments
                .Select(MapAssignment)
                .ToList()
        };
    }

    public async Task<StudentAssignmentViewModel?>
        GetAssignmentAsync(int assignmentId, string studentId)
    {
        var assignment = await _context.Assignments
            .AsNoTracking()
            .Where(item =>
                item.AssignmentId == assignmentId &&
                item.IsActive &&
                item.Course.IsActive &&
                item.Course.Enrollments.Any(enrollment =>
                    enrollment.StudentId == studentId &&
                    enrollment.Status == EnrollmentStatus.Active))
            .Include(item => item.Course)
            .Include(item => item.Submissions
                .Where(submission =>
                    submission.StudentId == studentId))
                .ThenInclude(submission => submission.Grade)
            .FirstOrDefaultAsync();

        return assignment is null
            ? null
            : MapAssignment(assignment);
    }

    public async Task<Submission?> GetSubmissionAsync(
        int submissionId,
        string studentId)
    {
        return await _context.Submissions
            .AsNoTracking()
            .Include(submission => submission.Assignment)
                .ThenInclude(assignment => assignment.Course)
            .Include(submission => submission.Grade)
            .FirstOrDefaultAsync(submission =>
                submission.SubmissionId == submissionId &&
                submission.StudentId == studentId);
    }

    public async Task<Submission> CreateSubmissionAsync(
        SubmissionViewModel model,
        string studentId)
    {
        var assignment = await _context.Assignments
            .AsNoTracking()
            .FirstOrDefaultAsync(item =>
                item.AssignmentId == model.AssignmentId &&
                item.IsActive &&
                item.Course.IsActive &&
                item.Course.Enrollments.Any(enrollment =>
                    enrollment.StudentId == studentId &&
                    enrollment.Status == EnrollmentStatus.Active));

        if (assignment is null)
        {
            throw new InvalidOperationException(
                "The assignment was not found or is unavailable.");
        }

        var submissionExists =
            await _context.Submissions.AnyAsync(submission =>
                submission.AssignmentId == model.AssignmentId &&
                submission.StudentId == studentId);

        if (submissionExists)
        {
            throw new InvalidOperationException(
                "You have already submitted this assignment.");
        }

        var submittedAt = DateTime.UtcNow;

        var submission = new Submission
        {
            AssignmentId = model.AssignmentId,
            StudentId = studentId,
            Content = model.Content.Trim(),
            SubmittedAt = submittedAt,
            IsLate = submittedAt > assignment.DueDate
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        return submission;
    }

    public async Task<bool> UpdateSubmissionAsync(
        SubmissionViewModel model,
        string studentId)
    {
        if (!model.SubmissionId.HasValue)
        {
            return false;
        }

        var submission = await _context.Submissions
            .Include(item => item.Assignment)
                .ThenInclude(assignment => assignment.Course)
                    .ThenInclude(course => course.Enrollments)
            .Include(item => item.Grade)
            .FirstOrDefaultAsync(item =>
                item.SubmissionId == model.SubmissionId.Value &&
                item.StudentId == studentId);

        if (submission is null ||
            submission.AssignmentId != model.AssignmentId ||
            !submission.Assignment.IsActive ||
            !submission.Assignment.Course.IsActive)
        {
            return false;
        }

        var hasActiveEnrollment =
            submission.Assignment.Course.Enrollments.Any(enrollment =>
                enrollment.StudentId == studentId &&
                enrollment.Status == EnrollmentStatus.Active);

        if (!hasActiveEnrollment)
        {
            return false;
        }

        if (submission.Grade is not null)
        {
            throw new InvalidOperationException(
                "A graded submission cannot be edited.");
        }

        var updatedAt = DateTime.UtcNow;

        submission.Content = model.Content.Trim();
        submission.UpdatedAt = updatedAt;
        submission.IsLate =
            updatedAt > submission.Assignment.DueDate;

        await _context.SaveChangesAsync();

        return true;
    }

    private static StudentAssignmentViewModel MapAssignment(
        Assignment assignment)
    {
        var submission = assignment.Submissions.SingleOrDefault();

        return new StudentAssignmentViewModel
        {
            AssignmentId = assignment.AssignmentId,
            CourseId = assignment.CourseId,
            CourseCode = assignment.Course.Code,
            CourseName = assignment.Course.Name,
            Title = assignment.Title,
            Description = assignment.Description,
            DueDate = assignment.DueDate,
            MaxPoints = assignment.MaxPoints,
            SubmissionId = submission?.SubmissionId,
            SubmittedAt = submission?.SubmittedAt,
            IsLate = submission?.IsLate ?? false,
            IsGraded = submission?.Grade is not null,
            Score = submission?.Grade?.Score,
            Feedback = submission?.Grade?.Feedback
        };
    }
}