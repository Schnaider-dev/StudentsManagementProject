using System.ComponentModel.DataAnnotations;

namespace StudentManager.Models;

public class Submission
{
    public int SubmissionId { get; set; }

    public int AssignmentId { get; set; }

    public Assignment Assignment { get; set; } = null!;

    [Required]
    public string StudentId { get; set; } = string.Empty;

    public ApplicationUser Student { get; set; } = null!;

    [Required]
    [StringLength(5000)]
    [Display(Name = "Submission Content")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Submitted At")]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Late Submission")]
    public bool IsLate { get; set; }

    [Display(Name = "Last Updated")]
    public DateTime? UpdatedAt { get; set; }

    public Grade? Grade { get; set; }
}