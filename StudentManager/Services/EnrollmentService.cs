using Microsoft.EntityFrameworkCore;
using StudentManager.Data;
using StudentManager.Models;
using StudentManager.Models.Enums;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Student;

namespace StudentManager.Services;

// Keeps the enrollment rules and the database queries used by student course pages.
public class EnrollmentService : IEnrollmentService
{
    // EF Core uses this context to build and run queries against the database.
    private readonly ApplicationDbContext _context;

    // Constructor injection gives this service the context configured by the application.
    public EnrollmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AvailableCoursesPageViewModel> GetAvailableCoursesAsync(
        string studentId,
        int page,
        int pageSize,
        string? search,
        string? filter)
    {
        // Page numbers start at 1; Math.Max prevents zero or negative page values.
        page = Math.Max(page, 1);

        // Keep page sizes useful and bounded so a request cannot ask for an excessive result set.
        pageSize = Math.Clamp(pageSize, 1, 50);

        // Trim accidental spaces; ?. means the call is skipped when the value is null.
        search = search?.Trim();
        filter = filter?.Trim().ToLowerInvariant();

        // Accept only filters that the view offers; anything else means search every field.
        if (filter is not ("code" or "name" or "teacher"))
        {
            filter = null;
        }

        // IQueryable builds a database query first; it does not fetch courses until an async
        // terminal operation such as CountAsync or ToListAsync runs it.
        var query = _context.Courses
            // Read-only pages do not need EF Core to track every returned entity for updates.
            .AsNoTracking()
            // Keep active courses and exclude courses where this student already has an
            // enrollment that is not dropped. 
            .Where(course =>
                course.IsActive &&
                !course.Enrollments.Any(enrollment => //Any checks whether a matching row exists.
                    enrollment.StudentId == studentId &&
                    enrollment.Status != EnrollmentStatus.Dropped));

        // Do not add search conditions for an empty search box.
        if (!string.IsNullOrWhiteSpace(search))
        {
            // A switch expression chooses one query condition based on the selected filter.
            query = filter switch
            {
                // Match the search text against only the requested field.
                "code" => query.Where(course => course.Code.Contains(search)),
                "name" => query.Where(course => course.Name.Contains(search)),
                "teacher" => query.Where(course =>
                    (course.Teacher.FirstName + " " + course.Teacher.LastName)
                    .Contains(search)),
                // A missing filter searches all three fields.
                _ => query.Where(course =>
                    course.Code.Contains(search) ||
                    course.Name.Contains(search) ||
                    (course.Teacher.FirstName + " " + course.Teacher.LastName)
                    .Contains(search))
            };
        }

        // Count after filtering so the page count describes only matching courses.
        var totalCount = await query.CountAsync();

        // Ceiling rounds a partial final page up; for example, 11 results at 10 per page
        // require 2 pages. A zero-result search correctly has 0 pages.
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // If results exist, move an out-of-range page back to the last real page.
        page = totalPages == 0 ? 1 : Math.Min(page, totalPages);

        // Sort before paging so records keep a stable order between page requests.
        var courses = await query
            .OrderBy(course => course.Code)
            .ThenBy(course => course.CourseId)
            // Skip the results on earlier pages, then fetch only this page's maximum size.
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            // Select copies database values into the smaller object needed by the view.
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
            // ToListAsync runs the composed query and returns its results.
            .ToListAsync();

        // Bundle the current results with the information the view needs to draw pagination.
        return new AvailableCoursesPageViewModel
        {
            Courses = courses,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Search = search,
            Filter = filter
        };
    }

    public async Task<IReadOnlyList<MyCourseViewModel>>
        GetStudentCoursesAsync(string studentId)
    {
        // Start from enrollment records so each result represents one of this student's courses.
        return await _context.Enrollments
            .AsNoTracking()
            // Only show active enrollments for courses that are still active.
            .Where(enrollment =>
                enrollment.StudentId == studentId &&
                enrollment.Status == EnrollmentStatus.Active &&
                enrollment.Course.IsActive)
            // Keep the list in a predictable course-code order.
            .OrderBy(enrollment => enrollment.Course.Code)
            // Convert each enrollment and related course data into the view's model.
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
            // Execute the database query and return the full enrolled-course list.
            .ToListAsync();
    }

    public async Task<bool> EnrollAsync(
        int courseId,
        string studentId)
    {
        // Reject invalid input before querying the database.
        if (courseId <= 0 || string.IsNullOrWhiteSpace(studentId))
        {
            return false;
        }

        // Do not create an enrollment for a missing or inactive course.
        var courseExists = await _context.Courses.AnyAsync(course =>
            course.CourseId == courseId &&
            course.IsActive);

        if (!courseExists)
        {
            return false;
        }

        // Reuse this student's prior enrollment if one exists for the course.
        var enrollment =
            await _context.Enrollments.FirstOrDefaultAsync(item =>
                item.CourseId == courseId &&
                item.StudentId == studentId);

        if (enrollment is null)
        {
            // First enrollment: create a new record with its initial active state and timestamp.
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
            // Active or completed enrollments cannot be enrolled a second time.
            if (enrollment.Status == EnrollmentStatus.Active ||
                enrollment.Status == EnrollmentStatus.Completed)
            {
                return false;
            }

            // A dropped enrollment can be reactivated instead of creating a duplicate record.
            enrollment.Status = EnrollmentStatus.Active;
            enrollment.EnrolledAt = DateTime.UtcNow;
            enrollment.DroppedAt = null;
        }

        // Persist either the new enrollment or the reactivated record.
        await _context.SaveChangesAsync();

        // true tells the controller the enrollment operation succeeded.
        return true;
    }

    public async Task<bool> DropAsync(
        int courseId,
        string studentId)
    {
        // Reject invalid input before querying the database.
        if (courseId <= 0 || string.IsNullOrWhiteSpace(studentId))
        {
            return false;
        }

        // Find only an active enrollment owned by this student for this course.
        var enrollment =
            await _context.Enrollments.FirstOrDefaultAsync(item =>
                item.CourseId == courseId &&
                item.StudentId == studentId &&
                item.Status == EnrollmentStatus.Active);

        if (enrollment is null)
        {
            return false;
        }

        // Keep the enrollment record as history, but mark it dropped and record when.
        enrollment.Status = EnrollmentStatus.Dropped;
        enrollment.DroppedAt = DateTime.UtcNow;

        // Save the status change to the database.
        await _context.SaveChangesAsync();

        // true tells the controller the drop operation succeeded.
        return true;
    }
}