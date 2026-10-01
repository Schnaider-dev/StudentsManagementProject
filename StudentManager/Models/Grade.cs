using System.ComponentModel.DataAnnotations;

namespace StudentManager.Models;

public class Grade
{
    public int GradeId { get; set; }

    public int SubmissionId { get; set; }

    public Submission Submission { get; set; } = null!;

    [Display(Name = "Score")]
    [Range(typeof(decimal), "0", "10000")]
    public decimal Score { get; set; }

    [StringLength(2000)]
    public string? Feedback { get; set; }

    [Required]
    public string GradedById { get; set; } = string.Empty;

    public ApplicationUser GradedBy { get; set; } = null!;

    [Display(Name = "Graded At")]
    public DateTime GradedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Last Updated")]
    public DateTime? UpdatedAt { get; set; }
}