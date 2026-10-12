using System.ComponentModel.DataAnnotations;

namespace StudentManager.Controllers.Api.Teacher;

/// <summary>Course information provided when a teacher creates a course.</summary>
public sealed record CreateCourseRequest
{
    [Required]
    [StringLength(20)]
    public required string Code { get; init; }

    [Required]
    [StringLength(100)]
    public required string Name { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }
}

/// <summary>Course information provided when a teacher updates a course.</summary>
public sealed record UpdateCourseRequest
{
    [Required]
    [StringLength(20)]
    public required string Code { get; init; }

    [Required]
    [StringLength(100)]
    public required string Name { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }
}
