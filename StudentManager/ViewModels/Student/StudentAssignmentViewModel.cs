namespace StudentManager.ViewModels.Student;

public class StudentAssignmentViewModel
{
    public int AssignmentId { get; set; }

    public int CourseId { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime DueDate { get; set; }

    public decimal MaxPoints { get; set; }

    public int? SubmissionId { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public bool IsLate { get; set; }

    public bool IsGraded { get; set; }

    public decimal? Score { get; set; }

    public string? Feedback { get; set; }
}