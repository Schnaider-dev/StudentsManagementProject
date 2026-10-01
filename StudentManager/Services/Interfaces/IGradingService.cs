using StudentManager.ViewModels.Teacher;

namespace StudentManager.Services.Interfaces;

public interface IGradingService
{
    Task<GradingListViewModel?> GetAssignmentSubmissionsAsync(
        int assignmentId,
        string teacherId);

    Task<GradeEditViewModel?> GetGradeFormAsync(
        int submissionId,
        string teacherId);

    Task<bool> SaveGradeAsync(
        GradeEditViewModel model,
        string teacherId);
}