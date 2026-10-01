namespace StudentManager.ViewModels.Teacher;

public class GradingSubmissionViewModel
{
    public int SubmissionId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string? StudentNumber { get; set; }

    public DateTime SubmittedAt { get; set; }

    public bool IsLate { get; set; }

    public bool IsGraded { get; set; }

    public decimal? Score { get; set; }

    public string? Feedback { get; set; }
}