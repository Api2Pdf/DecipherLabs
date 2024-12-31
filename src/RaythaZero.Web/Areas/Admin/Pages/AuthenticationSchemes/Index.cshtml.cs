using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.AuthenticationSchemes.Queries;
using RaythaZero.Application.Common.Utils;
using RaythaZero.Domain.Entities;
using RaythaZero.Domain.ValueObjects;
using RaythaZero.Web.Areas.Shared.Models;

namespace RaythaZero.Web.Areas.Admin.Pages.AuthenticationSchemes;

[Authorize(Policy = BuiltInSystemPermission.MANAGE_SYSTEM_SETTINGS_PERMISSION)]
public class Index : BaseAdminPageModel, IHasListView<Index.AuthenticationSchemesListItemViewModel>
{
    public ListViewModel<AuthenticationSchemesListItemViewModel> ListView { get; set; }
    public async Task<IActionResult> OnGet(
        string search = "", string orderBy = $"Label {SortOrder.ASCENDING}", int pageNumber = 1, int pageSize = 50)
    {
        var input = new GetAuthenticationSchemes.Query
        {
            Search = search,
            PageNumber = pageNumber,
            PageSize = pageSize,
            OrderBy = orderBy
        };

        var response = await Mediator.Send(input);

        var items = response.Result.Items.Select(p => new AuthenticationSchemesListItemViewModel
        {
            Id = p.Id,
            Label = p.Label,
            DeveloperName = p.DeveloperName,
            LastModificationTime = CurrentOrganization.TimeZoneConverter.UtcToTimeZoneAsDateTimeFormat(p.LastModificationTime),
            LastModifierUser = p.LastModifierUser != null ? p.LastModifierUser.FullName : "N/A",
            AuthenticationSchemeType = p.AuthenticationSchemeType.Label,
            IsEnabledForAdmins = p.IsEnabledForAdmins.YesOrNo(),
            IsEnabledForUsers = p.IsEnabledForUsers.YesOrNo()
        });

        ListView = new ListViewModel<AuthenticationSchemesListItemViewModel>(items, response.Result.TotalCount);
        return Page();
    }

    public record AuthenticationSchemesListItemViewModel
    {
        public string Id { get; init; }

        [Display(Name = "Label")]
        public string Label { get; init; }

        [Display(Name = "Developer name")]
        public string DeveloperName { get; init; }

        [Display(Name = "Last modified at")]
        public string LastModificationTime { get; init; }

        [Display(Name = "Last modified at")]
        public string LastModifierUser { get; init; }

        [Display(Name = "Auth scheme type")]
        public string AuthenticationSchemeType { get; init; }

        [Display(Name = "Is enabled for public users")]
        public string IsEnabledForUsers { get; set; }

        [Display(Name = "Is enabled for admins")]
        public string IsEnabledForAdmins { get; set; } 
    }
}