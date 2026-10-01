using System.ComponentModel.DataAnnotations;

namespace StudentManager.Models;

public class Course
{
    public int CourseId { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Course Code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Course Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public string TeacherId { get; set; } = string.Empty;

    public ApplicationUser Teacher { get; set; } = null!;

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Active Course")]
    public bool IsActive { get; set; } = true;

    public ICollection<Enrollment> Enrollments { get; set; } =
    new List<Enrollment>();

    public ICollection<Assignment> Assignments { get; set; } =
        new List<Assignment>();
}