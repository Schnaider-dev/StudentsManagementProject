namespace StudentManager.ViewModels.Teacher;

public interface ICourseFormModel
{
    string Code { get; set; }

    string Name { get; set; }

    string? Description { get; set; }
}
