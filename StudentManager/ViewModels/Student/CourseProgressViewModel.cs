namespace StudentManager.ViewModels.Student;

public class CourseProgressViewModel
{
    public int CourseId { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseName { get; set; } = string.Empty;

    public string TeacherName { get; set; } = string.Empty;

    public int TotalAssignments { get; set; }

    public int SubmittedAssignments { get; set; }

    public int GradedAssignments { get; set; }

    public decimal? AveragePercentage { get; set; }

    public IReadOnlyList<AssignmentProgressViewModel> Assignments
    { get; set; } = Array.Empty<AssignmentProgressViewModel>();
}