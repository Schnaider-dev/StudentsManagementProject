using Microsoft.EntityFrameworkCore;
using StudentManager.Data;
using StudentManager.Models;
using StudentManager.Models.Enums;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Student;

namespace StudentManager.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _context;

    public EnrollmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AvailableCourseViewModel>>
        GetAvailableCoursesAsync(string studentId)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(course =>
                course.IsActive &&
                !course.Enrollments.Any(enrollment =>
                    enrollment.StudentId == studentId &&
                    enrollment.Status != EnrollmentStatus.Dropped))
            .OrderBy(course => course.Code)
            .Select(course => new AvailableCourseViewModel
            {
                CourseId = course.CourseId,
                Code = course.Code,
                Name = course.Name,
                Description = course.Description,
                TeacherName =
                    (course.Teacher.FirstName + " " +
                     course.Teacher.LastName).Trim()
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<MyCourseViewModel>>
        GetStudentCoursesAsync(string studentId)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Where(enrollment =>
                enrollment.StudentId == studentId &&
                enrollment.Status == EnrollmentStatus.Active &&
                enrollment.Course.IsActive)
            .OrderBy(enrollment => enrollment.Course.Code)
            .Select(enrollment => new MyCourseViewModel
            {
                EnrollmentId = enrollment.EnrollmentId,
                CourseId = enrollment.CourseId,
                Code = enrollment.Course.Code,
                Name = enrollment.Course.Name,
                Description = enrollment.Course.Description,
                TeacherName =
                    (enrollment.Course.Teacher.FirstName + " " +
                     enrollment.Course.Teacher.LastName).Trim(),
                EnrolledAt = enrollment.EnrolledAt,
                AssignmentCount =
                    enrollment.Course.Assignments.Count(
                        assignment => assignment.IsActive)
            })
            .ToListAsync();
    }

    public async Task<bool> EnrollAsync(
        int courseId,
        string studentId)
    {
        if (courseId <= 0 || string.IsNullOrWhiteSpace(studentId))
        {
            return false;
        }

        var courseExists = await _context.Courses.AnyAsync(course =>
            course.CourseId == courseId &&
            course.IsActive);

        if (!courseExists)
        {
            return false;
        }

        var enrollment =
            await _context.Enrollments.FirstOrDefaultAsync(item =>
                item.CourseId == courseId &&
                item.StudentId == studentId);

        if (enrollment is null)
        {
            enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentId = studentId,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Active
            };

            _context.Enrollments.Add(enrollment);
        }
        else
        {
            if (enrollment.Status == EnrollmentStatus.Active ||
                enrollment.Status == EnrollmentStatus.Completed)
            {
                return false;
            }

            enrollment.Status = EnrollmentStatus.Active;
            enrollment.EnrolledAt = DateTime.UtcNow;
            enrollment.DroppedAt = null;
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DropAsync(
        int courseId,
        string studentId)
    {
        if (courseId <= 0 || string.IsNullOrWhiteSpace(studentId))
        {
            return false;
        }

        var enrollment =
            await _context.Enrollments.FirstOrDefaultAsync(item =>
                item.CourseId == courseId &&
                item.StudentId == studentId &&
                item.Status == EnrollmentStatus.Active);

        if (enrollment is null)
        {
            return false;
        }

        enrollment.Status = EnrollmentStatus.Dropped;
        enrollment.DroppedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}