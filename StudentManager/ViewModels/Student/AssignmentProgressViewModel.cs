namespace StudentManager.ViewModels.Student;

public class AssignmentProgressViewModel
{
    public int AssignmentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public decimal MaxPoints { get; set; }

    public bool IsSubmitted { get; set; }

    public bool IsGraded { get; set; }

    public decimal? Score { get; set; }

    public decimal? Percentage { get; set; }

    public string? Feedback { get; set; }
    public string? Grade { get; set; }
public string? Status { get; set; }

}