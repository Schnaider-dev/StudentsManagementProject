using StudentManager.Models;
using StudentManager.ViewModels.Teacher;

namespace StudentManager.Services.Interfaces;

public interface ICourseService
{
    Task<IReadOnlyList<Course>> GetTeacherCoursesAsync(
        string teacherId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    Task<Course?> GetTeacherCourseAsync(
        int courseId,
        string teacherId,
        CancellationToken cancellationToken = default);

    Task<bool> CourseCodeExistsAsync(
        string code,
        int? excludedCourseId = null,
        CancellationToken cancellationToken = default);

    Task<Course> CreateCourseAsync(
        CourseCreateViewModel model,
        string teacherId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateCourseAsync(
        CourseEditViewModel model,
        string teacherId,
        CancellationToken cancellationToken = default);

    Task<bool> ArchiveCourseAsync(
        int courseId,
        string teacherId,
        CancellationToken cancellationToken = default);
}
