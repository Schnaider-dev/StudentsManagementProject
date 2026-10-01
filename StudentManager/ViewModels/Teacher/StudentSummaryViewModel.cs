namespace StudentManager.ViewModels.Teacher;

public class StudentSummaryViewModel
{
    public string StudentId { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public string? StudentNumber { get; set; }

    public string Email { get; set; } = string.Empty;

    public DateTime EnrolledAt { get; set; }

    public int SubmittedAssignmentCount { get; set; }

    public int GradedAssignmentCount { get; set; }

    public decimal? AveragePercentage { get; set; }
}