using System.ComponentModel.DataAnnotations;

namespace StudentManager.Models;

public class Assignment
{
    public int AssignmentId { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "Due Date")]
    [DataType(DataType.DateTime)]
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);

    [Display(Name = "Maximum Points")]
    [Range(typeof(decimal), "0.01", "10000")]
    public decimal MaxPoints { get; set; } = 100;

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public ICollection<Submission> Submissions { get; set; } =
        new List<Submission>();
}
