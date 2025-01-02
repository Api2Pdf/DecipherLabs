using RaythaZero.Web.Areas.Shared.Models;

namespace RaythaZero.Web.Areas.Public.Pages.Projects;

public class Index : BasePublicPageModel, IHasListView<Index.ProjectsListItemViewModel>
{
    public ListViewModel<ProjectsListItemViewModel> ListView { get; set; }

    public void OnGet(bool archived = false)
    {
        ListView = new ListViewModel<ProjectsListItemViewModel>(new List<ProjectsListItemViewModel>()
        {
            new() { Id = "1", Label = "Project 1", IsArchived = false },
            new() { Id = "2", Label = "Project 2", IsArchived = false },
            new() { Id = "3", Label = "Project 3", IsArchived = true},
        }, 3);
    }

    public record ProjectsListItemViewModel
    {
        public string Id { get; init; }
        public string Label { get; init; }
        public bool IsArchived { get; init; }
    }
}