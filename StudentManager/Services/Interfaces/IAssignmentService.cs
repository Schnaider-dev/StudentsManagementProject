using StudentManager.Models;
using StudentManager.ViewModels.Teacher;

namespace StudentManager.Services.Interfaces;

public interface IAssignmentService
{
    Task<IReadOnlyList<Assignment>> GetCourseAssignmentsAsync(
        int courseId,
        string teacherId,
        bool includeInactive = false);

    Task<Assignment?> GetTeacherAssignmentAsync(
        int assignmentId,
        string teacherId);

    Task<Assignment> CreateAssignmentAsync(
        AssignmentCreateViewModel model,
        string teacherId);

    Task<bool> UpdateAssignmentAsync(
        AssignmentEditViewModel model,
        string teacherId);

    Task<bool> ArchiveAssignmentAsync(
        int assignmentId,
        string teacherId);
}