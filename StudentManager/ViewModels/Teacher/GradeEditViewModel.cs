using System.ComponentModel.DataAnnotations;

namespace StudentManager.ViewModels.Teacher;

public class GradeEditViewModel
{
    public int? GradeId { get; set; }

    [Range(1, int.MaxValue)]
    public int SubmissionId { get; set; }

    public int AssignmentId { get; set; }

    public string AssignmentTitle { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public decimal MaxPoints { get; set; }

    [Required]
    [Range(typeof(decimal), "0", "10000")]
    public decimal Score { get; set; }

    [StringLength(2000)]
    public string? Feedback { get; set; }
}