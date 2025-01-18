using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.Common.Utils;
using RaythaZero.Application.Prompts.Queries;
using RaythaZero.Domain.ValueObjects;
using RaythaZero.Web.Areas.Shared.Models;

namespace RaythaZero.Web.Areas.Admin.Pages.Prompts;

public class Index : BaseAdminPageModel, IHasListView<Index.PromptsListItemViewModel>
{
    public ListViewModel<PromptsListItemViewModel> ListView { get; set; }
    
    public async Task<IActionResult> OnGet(
        string search = "", string orderBy = $"Label {SortOrder.ASCENDING}", int pageNumber = 1, int pageSize = 50)
    {
        var input = new GetPrompts.Query
        {
            Search = search,
            OrderBy = orderBy,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var response = await Mediator.Send(input);

        var items = response.Result.Items.Select(p => new PromptsListItemViewModel
        {
            Id = p.Id,  
            CreationTime = CurrentOrganization.TimeZoneConverter.UtcToTimeZoneAsDateTimeFormat(p.CreationTime),
            LastModificationTime = CurrentOrganization.TimeZoneConverter.UtcToTimeZoneAsDateTimeFormat(p.LastModificationTime),
            IsActive = p.IsActive.YesOrNo(),
            ResultType = p.ResultType,
            Label = p.Label,
            DeveloperName = p.DeveloperName
        });

        ListView = new ListViewModel<PromptsListItemViewModel>(items, response.Result.TotalCount);
        return Page();
    }
    
    public record PromptsListItemViewModel
    {
        public string Id { get; init; }

        [Display(Name = "Label")]
        public string Label{ get; init; }

        [Display(Name = "Developer name")]
        public string DeveloperName { get; init; }

        [Display(Name = "Prompt text")]
        public string PromptText { get; init; }

        [Display(Name = "Created at")]
        public string CreationTime { get; init; }

        [Display(Name = "Last modified at")]
        public string LastModificationTime{ get; init; }

        [Display(Name = "Is active")]
        public string IsActive { get; init; }

        [Display(Name = "Result type")]
        public string ResultType{ get; init; }
    }
}