using System.ComponentModel.DataAnnotations;

namespace StudentManager.ViewModels.Teacher;

public class CourseEditViewModel
{
    [Required]
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
}