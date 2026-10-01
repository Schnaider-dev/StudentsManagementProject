namespace StudentManager.ViewModels.Student;

public class MyCourseViewModel
{
    public int EnrollmentId { get; set; }

    public int CourseId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string TeacherName { get; set; } = string.Empty;

    public DateTime EnrolledAt { get; set; }

    public int AssignmentCount { get; set; }
}