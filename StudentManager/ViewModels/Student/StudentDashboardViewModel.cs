namespace StudentManager.ViewModels.Student;

public class StudentDashboardViewModel
{
    public int ActiveCourseCount { get; set; }

    public int PendingAssignmentCount { get; set; }

    public int GradedAssignmentCount { get; set; }

    public decimal? OverallAveragePercentage { get; set; }
}