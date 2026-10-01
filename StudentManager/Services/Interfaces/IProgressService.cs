using StudentManager.ViewModels.Student;
using StudentManager.ViewModels.Teacher;

namespace StudentManager.Services.Interfaces;

public interface IProgressService
{
    Task<StudentDashboardViewModel> GetStudentDashboardAsync(
        string studentId);

    Task<CourseProgressViewModel?> GetCourseProgressAsync(
        int courseId,
        string studentId);

    Task<TeacherDashboardViewModel> GetTeacherDashboardAsync(
        string teacherId);

    Task<CourseStudentsViewModel?> GetCourseStudentsAsync(
        int courseId,
        string teacherId);
}