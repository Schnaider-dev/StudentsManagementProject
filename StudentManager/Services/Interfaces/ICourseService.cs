using StudentManager.Models;
using StudentManager.ViewModels.Teacher;

namespace StudentManager.Services.Interfaces;

public interface ICourseService
{
    Task<IReadOnlyList<Course>> GetTeacherCoursesAsync(
        string teacherId,
        bool includeInactive = false);

    Task<Course?> GetTeacherCourseAsync(
        int courseId,
        string teacherId);

    Task<bool> CourseCodeExistsAsync(
        string code,
        int? excludedCourseId = null);

    Task<Course> CreateCourseAsync(
        CourseCreateViewModel model,
        string teacherId);

    Task<bool> UpdateCourseAsync(
        CourseEditViewModel model,
        string teacherId);

    Task<bool> ArchiveCourseAsync(
        int courseId,
        string teacherId);
}
