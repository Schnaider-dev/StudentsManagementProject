namespace StudentManager.Controllers.Api.Teacher;

/// <summary>Course summary returned in a teacher's course list.</summary>
public sealed record TeacherCourseResponse(
    int CourseId,
    string Code,
    string Name,
    DateTimeOffset CreatedAt,
    bool IsActive,
    int StudentCount,
    int AssignmentCount);

/// <summary>Student summary shown on a course details response.</summary>
public sealed record CourseStudentResponse(
    string StudentId,
    string FirstName,
    string LastName,
    string? Email);

/// <summary>Assignment summary shown on a course details response.</summary>
public sealed record CourseAssignmentResponse(
    int AssignmentId,
    string Title,
    DateTimeOffset DueDate,
    int SubmissionCount);

/// <summary>Full course details returned to the course's teacher.</summary>
public sealed record TeacherCourseDetailsResponse(
    int CourseId,
    string Code,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    bool IsActive,
    int StudentCount,
    int ActiveAssignmentCount,
    IReadOnlyList<CourseStudentResponse> Students,
    IReadOnlyList<CourseAssignmentResponse> Assignments);
