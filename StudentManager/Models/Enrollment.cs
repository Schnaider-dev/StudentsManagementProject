using System.ComponentModel.DataAnnotations;
using StudentManager.Models.Enums;

namespace StudentManager.Models;

public class Enrollment
{
    public int EnrollmentId { get; set; }

    [Required]
    public string StudentId { get; set; } = string.Empty;

    public ApplicationUser Student { get; set; } = null!;

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    [Display(Name = "Enrollment Date")]
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public EnrollmentStatus Status { get; set; } =
        EnrollmentStatus.Active;

    [Display(Name = "Drop Date")]
    public DateTime? DroppedAt { get; set; }
}