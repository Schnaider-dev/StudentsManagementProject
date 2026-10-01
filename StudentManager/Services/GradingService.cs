using Microsoft.EntityFrameworkCore;
using StudentManager.Data;
using StudentManager.Models;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Teacher;

namespace StudentManager.Services;

public class GradingService : IGradingService
{
    private readonly ApplicationDbContext _context;

    public GradingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GradingListViewModel?>
        GetAssignmentSubmissionsAsync(
            int assignmentId,
            string teacherId)
    {
        var assignment = await _context.Assignments
            .AsNoTracking()
            .Where(item =>
                item.AssignmentId == assignmentId &&
                item.Course.TeacherId == teacherId)
            .Select(item => new
            {
                item.AssignmentId,
                item.Title,
                item.MaxPoints,
                item.CourseId,
                CourseCode = item.Course.Code
            })
            .FirstOrDefaultAsync();

        if (assignment is null)
        {
            return null;
        }

        var submissions = await _context.Submissions
            .AsNoTracking()
            .Where(submission =>
                submission.AssignmentId == assignmentId)
            .OrderBy(submission => submission.Student.LastName)
            .ThenBy(submission => submission.Student.FirstName)
            .Select(submission => new GradingSubmissionViewModel
            {
                SubmissionId = submission.SubmissionId,
                StudentName =
                    (submission.Student.FirstName + " " +
                     submission.Student.LastName).Trim(),
                StudentNumber = submission.Student.StudentNumber,
                SubmittedAt = submission.SubmittedAt,
                IsLate = submission.IsLate,
                IsGraded = submission.Grade != null,
                Score = submission.Grade == null
                    ? null
                    : submission.Grade.Score,
                Feedback = submission.Grade == null
                    ? null
                    : submission.Grade.Feedback
            })
            .ToListAsync();

        return new GradingListViewModel
        {
            CourseId = assignment.CourseId,
            CourseCode = assignment.CourseCode,
            AssignmentId = assignment.AssignmentId,
            AssignmentTitle = assignment.Title,
            MaxPoints = assignment.MaxPoints,
            Submissions = submissions
        };
    }

    public async Task<GradeEditViewModel?> GetGradeFormAsync(
        int submissionId,
        string teacherId)
    {
        var submission = await _context.Submissions
            .AsNoTracking()
            .Include(item => item.Student)
            .Include(item => item.Grade)
            .Include(item => item.Assignment)
                .ThenInclude(assignment => assignment.Course)
            .FirstOrDefaultAsync(item =>
                item.SubmissionId == submissionId &&
                item.Assignment.Course.TeacherId == teacherId);

        if (submission is null)
        {
            return null;
        }

        return new GradeEditViewModel
        {
            GradeId = submission.Grade?.GradeId,
            SubmissionId = submission.SubmissionId,
            AssignmentId = submission.AssignmentId,
            AssignmentTitle = submission.Assignment.Title,
            StudentName =
                $"{submission.Student.FirstName} " +
                $"{submission.Student.LastName}",
            MaxPoints = submission.Assignment.MaxPoints,
            Score = submission.Grade?.Score ?? 0,
            Feedback = submission.Grade?.Feedback
        };
    }

    public async Task<bool> SaveGradeAsync(
        GradeEditViewModel model,
        string teacherId)
    {
        var submission = await _context.Submissions
            .Include(item => item.Grade)
            .Include(item => item.Assignment)
                .ThenInclude(assignment => assignment.Course)
            .FirstOrDefaultAsync(item =>
                item.SubmissionId == model.SubmissionId &&
                item.Assignment.Course.TeacherId == teacherId);

        if (submission is null ||
            submission.AssignmentId != model.AssignmentId)
        {
            return false;
        }

        if (model.GradeId.HasValue &&
            submission.Grade?.GradeId != model.GradeId.Value)
        {
            return false;
        }

        if (model.Score < 0 ||
            model.Score > submission.Assignment.MaxPoints)
        {
            throw new InvalidOperationException(
                $"Score must be between 0 and " +
                $"{submission.Assignment.MaxPoints}.");
        }

        if (submission.Grade is null)
        {
            submission.Grade = new Grade
            {
                SubmissionId = submission.SubmissionId,
                Score = model.Score,
                Feedback = NormalizeOptionalText(model.Feedback),
                GradedById = teacherId,
                GradedAt = DateTime.UtcNow
            };
        }
        else
        {
            submission.Grade.Score = model.Score;
            submission.Grade.Feedback =
                NormalizeOptionalText(model.Feedback);
            submission.Grade.GradedById = teacherId;
            submission.Grade.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return true;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}