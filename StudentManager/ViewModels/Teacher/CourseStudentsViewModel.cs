namespace StudentManager.ViewModels.Teacher;

public class CourseStudentsViewModel
{
    public int CourseId { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseName { get; set; } = string.Empty;

    public IReadOnlyList<StudentSummaryViewModel> Students
    { get; set; } = Array.Empty<StudentSummaryViewModel>();
}