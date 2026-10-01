using StudentManager.ViewModels.Student;

namespace StudentManager.Services.Interfaces;

public interface IEnrollmentService
{
    Task<IReadOnlyList<AvailableCourseViewModel>>
        GetAvailableCoursesAsync(string studentId);

    Task<IReadOnlyList<MyCourseViewModel>>
        GetStudentCoursesAsync(string studentId);

    Task<bool> EnrollAsync(
        int courseId,
        string studentId);

    Task<bool> DropAsync(
        int courseId,
        string studentId);
}