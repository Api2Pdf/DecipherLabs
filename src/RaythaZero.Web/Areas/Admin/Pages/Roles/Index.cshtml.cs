using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.Roles.Queries;
using RaythaZero.Domain.Entities;
using RaythaZero.Domain.ValueObjects;
using RaythaZero.Web.Areas.Shared.Models;

namespace RaythaZero.Web.Areas.Admin.Pages.Roles;

[Authorize(Policy = BuiltInSystemPermission.MANAGE_ADMINISTRATORS_PERMISSION)]
public class Index : BaseAdminPageModel, IHasListView<Index.RolesListItemViewModel>
{
    public ListViewModel<RolesListItemViewModel> ListView { get; set; }
    public async Task<IActionResult> OnGet(
        string search = "", string orderBy = $"Label {SortOrder.ASCENDING}", int pageNumber = 1, int pageSize = 50)
    {
        var input = new GetRoles.Query
        {
            Search = search,
            PageNumber = pageNumber,
            PageSize = pageSize,
            OrderBy = orderBy
        };

        var response = await Mediator.Send(input);
        var items = response.Result.Items.Select(p => new RolesListItemViewModel
        {
            DeveloperName = p.DeveloperName,
            Label = p.Label,
            Id = p.Id,
        });

        ListView = new ListViewModel<RolesListItemViewModel>(items, response.Result.TotalCount);
        return Page();
    }

    public record RolesListItemViewModel
    {
        public string Id { get; init; }
        
        [Display(Name = "Label")]
        public string Label { get; init; }

        [Display(Name = "DeveloperName")]
        public string DeveloperName { get; init; } 
    }
}