using Microsoft.EntityFrameworkCore;
using StudentManager.Data;
using StudentManager.Models;
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
        bool includeInactive = false)
    {
        var query = _context.Courses
            .AsNoTracking()
            .Where(course => course.TeacherId == teacherId);

        if (!includeInactive)
        {
            query = query.Where(course => course.IsActive);
        }

        return await query
            .Include(course => course.Enrollments)
            .Include(course => course.Assignments)
            .OrderBy(course => course.Code)
            .ToListAsync();
    }

    public async Task<Course?> GetTeacherCourseAsync(
        int courseId,
        string teacherId)
    {
        return await _context.Courses
            .AsNoTracking()
            .Include(course => course.Enrollments)
                .ThenInclude(enrollment => enrollment.Student)
            .Include(course => course.Assignments)
                .ThenInclude(assignment => assignment.Submissions)
            .FirstOrDefaultAsync(course =>
                course.CourseId == courseId &&
                course.TeacherId == teacherId);
    }

    public async Task<bool> CourseCodeExistsAsync(
        string code,
        int? excludedCourseId = null)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        return await _context.Courses.AnyAsync(course =>
            course.Code.ToUpper() == normalizedCode &&
            (!excludedCourseId.HasValue ||
             course.CourseId != excludedCourseId.Value));
    }

    public async Task<Course> CreateCourseAsync(
        CourseCreateViewModel model,
        string teacherId)
    {
        var normalizedCode = model.Code.Trim().ToUpperInvariant();

        if (await CourseCodeExistsAsync(normalizedCode))
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
        await _context.SaveChangesAsync();

        return course;
    }

    public async Task<bool> UpdateCourseAsync(
        CourseEditViewModel model,
        string teacherId)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(course =>
            course.CourseId == model.CourseId &&
            course.TeacherId == teacherId);

        if (course is null)
        {
            return false;
        }

        var normalizedCode = model.Code.Trim().ToUpperInvariant();

        if (await CourseCodeExistsAsync(
                normalizedCode,
                model.CourseId))
        {
            throw new InvalidOperationException(
                $"A course with code '{normalizedCode}' already exists.");
        }

        course.Code = normalizedCode;
        course.Name = model.Name.Trim();
        course.Description = NormalizeOptionalText(model.Description);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ArchiveCourseAsync(
        int courseId,
        string teacherId)
    {
        var course = await _context.Courses
            .Include(course => course.Assignments)
            .FirstOrDefaultAsync(course =>
                course.CourseId == courseId &&
                course.TeacherId == teacherId);

        if (course is null)
        {
            return false;
        }

        course.IsActive = false;

        foreach (var assignment in course.Assignments)
        {
            assignment.IsActive = false;
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
