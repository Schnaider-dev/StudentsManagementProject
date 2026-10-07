using Microsoft.EntityFrameworkCore;
using StudentManager.Data;
using StudentManager.Models.Enums;
using StudentManager.Models;
using StudentManager.Services.Interfaces;
using StudentManager.ViewModels.Student;
using StudentManager.ViewModels.Teacher;

namespace StudentManager.Services;

public class ProgressService : IProgressService
{
    private readonly ApplicationDbContext _context;

    public ProgressService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentDashboardViewModel>
        GetStudentDashboardAsync(string studentId)
    {
        var activeCourseCount = await _context.Enrollments
            .AsNoTracking()
            .CountAsync(enrollment =>
                enrollment.StudentId == studentId &&
                enrollment.Status == EnrollmentStatus.Active &&
                enrollment.Course.IsActive);

        var activeCourses = await _context.Enrollments
            .AsNoTracking()
            .Where(enrollment =>
                enrollment.StudentId == studentId &&
                enrollment.Status == EnrollmentStatus.Active &&
                enrollment.Course.IsActive)
            .Select(enrollment => new Course
            {
                CourseId = enrollment.CourseId,
                Code = enrollment.Course.Code,
                Name = enrollment.Course.Name
            })
            .Distinct()
            .ToListAsync();

        var pendingAssignmentCount = await _context.Assignments
            .AsNoTracking()
            .CountAsync(assignment =>
                assignment.IsActive &&
                assignment.Course.IsActive &&
                assignment.Course.Enrollments.Any(enrollment =>
                    enrollment.StudentId == studentId &&
                    enrollment.Status == EnrollmentStatus.Active) &&
                !assignment.Submissions.Any(submission =>
                    submission.StudentId == studentId));

        var pendingAssignments = await _context.Assignments
            .AsNoTracking()
            .Where(assignment =>
                assignment.IsActive &&
                assignment.Course.IsActive &&
                assignment.Course.Enrollments.Any(enrollment =>
                    enrollment.StudentId == studentId &&
                    enrollment.Status == EnrollmentStatus.Active) &&
                !assignment.Submissions.Any(submission =>
                    submission.StudentId == studentId))
            .OrderBy(assignment => assignment.DueDate)
            .ToListAsync();

        var gradedResults = await _context.Grades
            .AsNoTracking()
            .Where(grade =>
                grade.Submission.StudentId == studentId &&
                grade.Submission.Assignment.IsActive &&
                grade.Submission.Assignment.Course.IsActive &&
                grade.Submission.Assignment.Course.Enrollments.Any(
                    enrollment =>
                        enrollment.StudentId == studentId &&
                        enrollment.Status ==
                            EnrollmentStatus.Active))
            .Select(grade => new
            {
                grade.Score,
                grade.Submission.Assignment.MaxPoints
            })
            .ToListAsync();

        var earnedPoints = gradedResults.Sum(item => item.Score);
        var possiblePoints =
            gradedResults.Sum(item => item.MaxPoints);

        return new StudentDashboardViewModel
        {
            ActiveCourseCount = activeCourseCount,
            PendingAssignmentCount = pendingAssignmentCount,
            PendingAssignments = pendingAssignments,
            GradedAssignmentCount = gradedResults.Count,
            OverallAveragePercentage = CalculatePercentage(
                earnedPoints,
                possiblePoints),
            ActiveCourses = activeCourses
        };
    }

    public async Task<CourseProgressViewModel?>
        GetCourseProgressAsync(
            int courseId,
            string studentId)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Where(item =>
                item.CourseId == courseId &&
                item.IsActive &&
                item.Enrollments.Any(enrollment =>
                    enrollment.StudentId == studentId &&
                    enrollment.Status ==
                        EnrollmentStatus.Active))
            .Select(item => new
            {
                item.CourseId,
                item.Code,
                item.Name,
                TeacherFirstName = item.Teacher.FirstName,
                TeacherLastName = item.Teacher.LastName
            })
            .FirstOrDefaultAsync();

        if (course is null)
        {
            return null;
        }

        var assignments = await _context.Assignments
            .AsNoTracking()
            .Where(assignment =>
                assignment.CourseId == courseId &&
                assignment.IsActive)
            .Include(assignment => assignment.Submissions
                .Where(submission =>
                    submission.StudentId == studentId))
                .ThenInclude(submission => submission.Grade)
            .OrderBy(assignment => assignment.DueDate)
            .ToListAsync();

        var assignmentModels = assignments
            .Select(assignment =>
            {
                var submission =
                    assignment.Submissions.SingleOrDefault();
                var grade = submission?.Grade;

                return new AssignmentProgressViewModel
                {
                    AssignmentId = assignment.AssignmentId,
                    Title = assignment.Title,
                    DueDate = assignment.DueDate,
                    MaxPoints = assignment.MaxPoints,
                    IsSubmitted = submission is not null,
                    IsGraded = grade is not null,
                    Score = grade?.Score,
                    Percentage = grade is null
                        ? null
                        : CalculatePercentage(
                            grade.Score,
                            assignment.MaxPoints),
                    Feedback = grade?.Feedback
                };
            })
            .ToList();

        var gradedAssignments = assignmentModels
            .Where(item => item.Score.HasValue)
            .ToList();

        var earnedPoints = gradedAssignments.Sum(
            item => item.Score!.Value);

        var possiblePoints = gradedAssignments.Sum(
            item => item.MaxPoints);

        return new CourseProgressViewModel
        {
            CourseId = course.CourseId,
            CourseCode = course.Code,
            CourseName = course.Name,
            TeacherName =
                $"{course.TeacherFirstName} " +
                $"{course.TeacherLastName}",
            TotalAssignments = assignmentModels.Count,
            SubmittedAssignments = assignmentModels.Count(
                item => item.IsSubmitted),
            GradedAssignments = gradedAssignments.Count,
            AveragePercentage = CalculatePercentage(
                earnedPoints,
                possiblePoints),
            Assignments = assignmentModels
        };
    }

    public async Task<TeacherDashboardViewModel>
        GetTeacherDashboardAsync(string teacherId)
    {
        var activeCourseCount = await _context.Courses
            .AsNoTracking()
            .CountAsync(course =>
                course.TeacherId == teacherId &&
                course.IsActive);

        var activeStudentCount = await _context.Enrollments
            .AsNoTracking()
            .Where(enrollment =>
                enrollment.Course.TeacherId == teacherId &&
                enrollment.Course.IsActive &&
                enrollment.Status == EnrollmentStatus.Active)
            .Select(enrollment => enrollment.StudentId)
            .Distinct()
            .CountAsync();

        var activeAssignmentCount = await _context.Assignments
            .AsNoTracking()
            .CountAsync(assignment =>
                assignment.Course.TeacherId == teacherId &&
                assignment.Course.IsActive &&
                assignment.IsActive);

        var ungradedSubmissionCount =
            await _context.Submissions
                .AsNoTracking()
                .CountAsync(submission =>
                    submission.Assignment.Course.TeacherId ==
                        teacherId &&
                    submission.Assignment.Course.IsActive &&
                    submission.Assignment.IsActive &&
                    submission.Grade == null);

        return new TeacherDashboardViewModel
        {
            ActiveCourseCount = activeCourseCount,
            ActiveStudentCount = activeStudentCount,
            ActiveAssignmentCount = activeAssignmentCount,
            UngradedSubmissionCount = ungradedSubmissionCount
        };
    }

    public async Task<CourseStudentsViewModel?>
        GetCourseStudentsAsync(
            int courseId,
            string teacherId)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Where(item =>
                item.CourseId == courseId &&
                item.TeacherId == teacherId)
            .Select(item => new
            {
                item.CourseId,
                item.Code,
                item.Name
            })
            .FirstOrDefaultAsync();

        if (course is null)
        {
            return null;
        }

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(enrollment =>
                enrollment.CourseId == courseId &&
                enrollment.Status == EnrollmentStatus.Active)
            .Include(enrollment => enrollment.Student)
            .OrderBy(enrollment => enrollment.Student.LastName)
            .ThenBy(enrollment => enrollment.Student.FirstName)
            .ToListAsync();

        var studentIds = enrollments
            .Select(enrollment => enrollment.StudentId)
            .ToList();

        var submissions = studentIds.Count == 0
            ? []
            : await _context.Submissions
                .AsNoTracking()
                .Where(submission =>
                    submission.Assignment.CourseId == courseId &&
                    submission.Assignment.IsActive &&
                    studentIds.Contains(submission.StudentId))
                .Include(submission => submission.Assignment)
                .Include(submission => submission.Grade)
                .ToListAsync();

        var students = enrollments
            .Select(enrollment =>
            {
                var studentSubmissions = submissions
                    .Where(submission =>
                        submission.StudentId ==
                            enrollment.StudentId)
                    .ToList();

                var gradedSubmissions = studentSubmissions
                    .Where(submission =>
                        submission.Grade is not null)
                    .ToList();

                var earnedPoints = gradedSubmissions.Sum(
                    submission => submission.Grade!.Score);

                var possiblePoints = gradedSubmissions.Sum(
                    submission =>
                        submission.Assignment.MaxPoints);

                return new StudentSummaryViewModel
                {
                    StudentId = enrollment.StudentId,
                    StudentName =
                        $"{enrollment.Student.FirstName} " +
                        $"{enrollment.Student.LastName}",
                    StudentNumber =
                        enrollment.Student.StudentNumber,
                    Email = enrollment.Student.Email ??
                        string.Empty,
                    EnrolledAt = enrollment.EnrolledAt,
                    SubmittedAssignmentCount =
                        studentSubmissions.Count,
                    GradedAssignmentCount =
                        gradedSubmissions.Count,
                    AveragePercentage = CalculatePercentage(
                        earnedPoints,
                        possiblePoints)
                };
            })
            .ToList();

        return new CourseStudentsViewModel
        {
            CourseId = course.CourseId,
            CourseCode = course.Code,
            CourseName = course.Name,
            Students = students
        };
    }

    private static decimal? CalculatePercentage(
        decimal earnedPoints,
        decimal possiblePoints)
    {
        if (possiblePoints <= 0)
        {
            return null;
        }

        return Math.Round(
            earnedPoints / possiblePoints * 100,
            2);
    }
}