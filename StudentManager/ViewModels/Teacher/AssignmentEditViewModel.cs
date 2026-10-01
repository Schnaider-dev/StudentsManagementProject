using System.ComponentModel.DataAnnotations;

namespace StudentManager.ViewModels.Teacher;

public class AssignmentEditViewModel
{
    [Range(1, int.MaxValue)]
    public int AssignmentId { get; set; }

    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Due Date")]
    [DataType(DataType.DateTime)]
    public DateTime DueDate { get; set; }

    [Required]
    [Display(Name = "Maximum Points")]
    [Range(typeof(decimal), "0.01", "10000")]
    public decimal MaxPoints { get; set; }
}