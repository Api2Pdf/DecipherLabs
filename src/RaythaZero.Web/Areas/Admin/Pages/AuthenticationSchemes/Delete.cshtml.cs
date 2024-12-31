using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.AuthenticationSchemes.Commands;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Web.Areas.Admin.Pages.AuthenticationSchemes;

[Authorize(Policy = BuiltInSystemPermission.MANAGE_SYSTEM_SETTINGS_PERMISSION)]
public class Delete : BaseAdminPageModel
{
    public async Task<IActionResult> OnPost(string id)
    {
        var input = new DeleteAuthenticationScheme.Command { Id = id };
        var response = await Mediator.Send(input);
        if (response.Success)
        {
            SetSuccessMessage($"Authentication scheme has been deleted.");
            return RedirectToPage("/AuthenticationSchemes/Index");
        }
        else
        {
            SetErrorMessage("There was an error deleting this authentication scheme", response.GetErrors());
            return RedirectToPage("/AuthenticationSchemes/Edit", new { id });
        }
    }
}