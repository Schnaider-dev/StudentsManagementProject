using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StudentManager.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "Student Number")]
    public string? StudentNumber { get; set; }

    [Display(Name = "Account Created")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Course> CoursesTaught { get; set; } =
    new List<Course>();

    public ICollection<Enrollment> Enrollments { get; set; } =
        new List<Enrollment>();

    public ICollection<Submission> Submissions { get; set; } =
        new List<Submission>();

    public ICollection<Grade> GradesGiven { get; set; } =
        new List<Grade>();
}