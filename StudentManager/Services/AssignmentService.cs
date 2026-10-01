using Microsoft.EntityFrameworkCore;
using StudentManager.Data;
using StudentManager.Models;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Teacher;

namespace StudentManager.Services;

public class AssignmentService : IAssignmentService
{
    private readonly ApplicationDbContext _context;

    public AssignmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Assignment>>
        GetCourseAssignmentsAsync(
            int courseId,
            string teacherId,
            bool includeInactive = false)
    {
        var query = _context.Assignments
            .AsNoTracking()
            .Where(assignment =>
                assignment.CourseId == courseId &&
                assignment.Course.TeacherId == teacherId);

        if (!includeInactive)
        {
            query = query.Where(assignment => assignment.IsActive);
        }

        return await query
            .Include(assignment => assignment.Course)
            .Include(assignment => assignment.Submissions)
                .ThenInclude(submission => submission.Grade)
            .OrderBy(assignment => assignment.DueDate)
            .ToListAsync();
    }

    public async Task<Assignment?> GetTeacherAssignmentAsync(
        int assignmentId,
        string teacherId)
    {
        return await _context.Assignments
            .AsNoTracking()
            .Include(assignment => assignment.Course)
            .Include(assignment => assignment.Submissions)
                .ThenInclude(submission => submission.Student)
            .Include(assignment => assignment.Submissions)
                .ThenInclude(submission => submission.Grade)
            .FirstOrDefaultAsync(assignment =>
                assignment.AssignmentId == assignmentId &&
                assignment.Course.TeacherId == teacherId);
    }

    public async Task<Assignment> CreateAssignmentAsync(
        AssignmentCreateViewModel model,
        string teacherId)
    {
        var courseExists = await _context.Courses.AnyAsync(course =>
            course.CourseId == model.CourseId &&
            course.TeacherId == teacherId &&
            course.IsActive);

        if (!courseExists)
        {
            throw new InvalidOperationException(
                "The selected course was not found or is inactive.");
        }

        var assignment = new Assignment
        {
            CourseId = model.CourseId,
            Title = model.Title.Trim(),
            Description = NormalizeOptionalText(model.Description),
            DueDate = model.DueDate,
            MaxPoints = model.MaxPoints,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();

        return assignment;
    }

    public async Task<bool> UpdateAssignmentAsync(
        AssignmentEditViewModel model,
        string teacherId)
    {
        var assignment = await _context.Assignments
            .Include(item => item.Course)
            .FirstOrDefaultAsync(item =>
                item.AssignmentId == model.AssignmentId &&
                item.Course.TeacherId == teacherId &&
                item.IsActive);

        if (assignment is null ||
            assignment.CourseId != model.CourseId)
        {
            return false;
        }

        assignment.Title = model.Title.Trim();
        assignment.Description =
            NormalizeOptionalText(model.Description);
        assignment.DueDate = model.DueDate;
        assignment.MaxPoints = model.MaxPoints;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ArchiveAssignmentAsync(
        int assignmentId,
        string teacherId)
    {
        var assignment = await _context.Assignments
            .Include(item => item.Course)
            .FirstOrDefaultAsync(item =>
                item.AssignmentId == assignmentId &&
                item.Course.TeacherId == teacherId);

        if (assignment is null)
        {
            return false;
        }

        assignment.IsActive = false;

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