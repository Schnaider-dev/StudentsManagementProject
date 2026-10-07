using System.ComponentModel.DataAnnotations;
using StudentManager.Models;

namespace StudentManager.ViewModels.Student;

public class StudentDashboardViewModel
{
    public int ActiveCourseCount { get; set; } = 0;

    public int PendingAssignmentCount { get; set; } = 0;

    public int GradedAssignmentCount { get; set; } = 0;

    public decimal? OverallAveragePercentage { get; set; } = 0;

    public List<Course>? ActiveCourses { get; set; }

    public List<Assignment>? PendingAssignments { get; set; }
}