using StudentManager.Models;
using StudentManager.ViewModels.Student;

namespace StudentManager.Services.Interfaces;

public interface ISubmissionService
{
    Task<CourseAssignmentsViewModel?> GetCourseAssignmentsAsync(
        int courseId,
        string studentId);

    Task<StudentAssignmentViewModel?> GetAssignmentAsync(
        int assignmentId,
        string studentId);

    Task<Submission?> GetSubmissionAsync(
        int submissionId,
        string studentId);

    Task<Submission> CreateSubmissionAsync(
        SubmissionViewModel model,
        string studentId);

    Task<bool> UpdateSubmissionAsync(
        SubmissionViewModel model,
        string studentId);
}