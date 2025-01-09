using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.Projects.Queries;
using RaythaZero.Domain.ValueObjects;
using RaythaZero.Web.Areas.Shared.Models;

namespace RaythaZero.Web.Areas.Public.Pages.Projects;

public class Index : BasePublicPageModel, IHasListView<Index.ProjectsListItemViewModel>
{
    public ListViewModel<ProjectsListItemViewModel> ListView { get; set; }
    public async Task<IActionResult> OnGet(
        string search = "", string orderBy = $"CreationTime {SortOrder.DESCENDING}", int pageNumber = 1, int pageSize = 50, bool showArchived = false)
    {
        var input = new GetProjects.Query
        {
            Search = search,
            OrderBy = orderBy,
            PageNumber = pageNumber,
            PageSize = pageSize,
            ShowArchived = showArchived
        };

        var response = await Mediator.Send(input);

        var items = response.Result.Items.Select(p => new ProjectsListItemViewModel 
        {
            Id = p.Id,  
            Label = p.Label,
            IsArchived = p.IsArchived
        });

        ListView = new ListViewModel<ProjectsListItemViewModel>(items, response.Result.TotalCount);
        return Page();
    }

    public record ProjectsListItemViewModel
    {
        public string Id { get; init; }
        public string Label { get; init; }
        public bool? IsArchived { get; init; }
    }
}