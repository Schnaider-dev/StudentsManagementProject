namespace StudentManager.ViewModels.Teacher;

public class GradingListViewModel
{
    public int CourseId { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public int AssignmentId { get; set; }

    public string AssignmentTitle { get; set; } = string.Empty;

    public decimal MaxPoints { get; set; }

    public IReadOnlyList<GradingSubmissionViewModel> Submissions
    { get; set; } = Array.Empty<GradingSubmissionViewModel>();
}