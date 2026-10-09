namespace StudentManager.ViewModels.Student;

// Carries both the current page's courses and the values needed to build page navigation.
public class AvailableCoursesPageViewModel
{
    // Only these course records are rendered on the current page.
    public IReadOnlyList<AvailableCourseViewModel> Courses { get; set; } =
        Array.Empty<AvailableCourseViewModel>();

    // CurrentPage is one-based, so the first page is page 1 rather than page 0.
    public int CurrentPage { get; set; }

    // PageSize is the maximum number of course rows in Courses.
    public int PageSize { get; set; }

    // TotalCount counts matching available courses across every page.
    public int TotalCount { get; set; }

    // TotalPages is calculated from TotalCount and PageSize.
    public int TotalPages { get; set; }

    // Keep the search text so the view can put it back in the input and page links.
    public string? Search { get; set; }

    // Keep the chosen search field so the view can leave that option selected.
    public string? Filter { get; set; }

    // These computed properties save the view from repeating page-boundary comparisons.
    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;
}