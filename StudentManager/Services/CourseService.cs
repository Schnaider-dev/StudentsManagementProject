using Microsoft.EntityFrameworkCore;
using StudentManager.Data;
using StudentManager.Models;
using StudentManager.Models.Enums;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Teacher;

namespace StudentManager.Services;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _context;

    public CourseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Course>> GetTeacherCoursesAsync(
        string teacherId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        // Scope every teacher-facing lookup to the signed-in teacher's own records.
        var query = _context.Courses
            .AsNoTracking()
            .Where(course => course.TeacherId == teacherId);

        if (!includeInactive)
        {
            query = query.Where(course => course.IsActive);
        }

        return await query
            .Include(course => course.Enrollments
                .Where(enrollment =>
                    enrollment.Status != EnrollmentStatus.Dropped))
            .Include(course => course.Assignments)
            .OrderBy(course => course.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<Course?> GetTeacherCourseAsync(
        int courseId,
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .Include(course => course.Enrollments
                .Where(enrollment =>
                    enrollment.Status != EnrollmentStatus.Dropped))
                .ThenInclude(enrollment => enrollment.Student)
            .Include(course => course.Assignments)
                .ThenInclude(assignment => assignment.Submissions)
            .FirstOrDefaultAsync(course =>
                course.CourseId == courseId &&
                course.TeacherId == teacherId,
                cancellationToken);
    }

    public async Task<bool> CourseCodeExistsAsync(
        string code,
        int? excludedCourseId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        return await _context.Courses.AnyAsync(course =>
            course.Code.ToUpper() == normalizedCode &&
            (!excludedCourseId.HasValue ||
             course.CourseId != excludedCourseId.Value),
            cancellationToken);
    }

    public async Task<Course> CreateCourseAsync(
        CourseCreateViewModel model,
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        // Store course codes in one canonical form so case differences cannot create duplicates.
        var normalizedCode = model.Code.Trim().ToUpperInvariant();

        if (await CourseCodeExistsAsync(
                normalizedCode,
                cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException(
                $"A course with code '{normalizedCode}' already exists.");
        }

        var course = new Course
        {
            Code = normalizedCode,
            Name = model.Name.Trim(),
            Description = NormalizeOptionalText(model.Description),
            TeacherId = teacherId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync(cancellationToken);

        return course;
    }

    public async Task<bool> UpdateCourseAsync(
        CourseEditViewModel model,
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(course =>
            course.CourseId == model.CourseId &&
            course.TeacherId == teacherId,
            cancellationToken);

        if (course is null)
        {
            return false;
        }

        var normalizedCode = model.Code.Trim().ToUpperInvariant();

        if (await CourseCodeExistsAsync(
                normalizedCode,
                model.CourseId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                $"A course with code '{normalizedCode}' already exists.");
        }

        course.Code = normalizedCode;
        course.Name = model.Name.Trim();
        course.Description = NormalizeOptionalText(model.Description);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ArchiveCourseAsync(
        int courseId,
        string teacherId,
        CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses
            .Include(course => course.Assignments)
            .FirstOrDefaultAsync(course =>
                course.CourseId == courseId &&
                course.TeacherId == teacherId,
                cancellationToken);

        if (course is null)
        {
            return false;
        }

        // Preserve course and assignment history instead of deleting referenced records.
        course.IsActive = false;

        foreach (var assignment in course.Assignments)
        {
            assignment.IsActive = false;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
