using StudentManager.ViewModels.Student;

namespace StudentManager.Services.Interfaces;

// Describes the enrollment operations controllers can request without depending on
// the database implementation directly.
public interface IEnrollmentService
{
    // Returns one filtered page of courses the student may enroll in.
    Task<AvailableCoursesPageViewModel> GetAvailableCoursesAsync(
        string studentId,
        // Page is 1-based; pageSize controls how many results are returned.
        int page,
        int pageSize,
        // Search is the entered text; filter selects which course field to search.
        string? search,
        string? filter);

    // Returns all active courses currently enrolled by this student.
    Task<IReadOnlyList<MyCourseViewModel>>
        GetStudentCoursesAsync(string studentId);

    // Attempts to enroll this student in a course; false means it was not allowed or failed.
    Task<bool> EnrollAsync(
        int courseId,
        string studentId);

    // Attempts to drop this student's active course enrollment.
    Task<bool> DropAsync(
        int courseId,
        string studentId);
}