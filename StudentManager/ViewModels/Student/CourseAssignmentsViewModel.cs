namespace StudentManager.ViewModels.Student;

public class CourseAssignmentsViewModel
{
    public int CourseId { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseName { get; set; } = string.Empty;

    public IReadOnlyList<StudentAssignmentViewModel> Assignments
    { get; set; } = Array.Empty<StudentAssignmentViewModel>();
}