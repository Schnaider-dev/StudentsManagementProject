using System.ComponentModel.DataAnnotations;

namespace StudentManager.ViewModels.Student;

public class SubmissionViewModel
{
    public int? SubmissionId { get; set; }

    [Range(1, int.MaxValue)]
    public int AssignmentId { get; set; }

    public string AssignmentTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    [Display(Name = "Submission Content")]
    public string Content { get; set; } = string.Empty;
}