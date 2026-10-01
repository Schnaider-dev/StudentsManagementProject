using StudentManager.Models;

namespace StudentManager.ViewModels.Teacher;

public class AssignmentIndexViewModel
{
    public int CourseId { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseName { get; set; } = string.Empty;

    public IReadOnlyList<Assignment> Assignments { get; set; } =
        Array.Empty<Assignment>();
}