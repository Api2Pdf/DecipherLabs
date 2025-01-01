using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.Users.Commands;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Web.Areas.Admin.Pages.Users;

[Authorize(Policy = BuiltInSystemPermission.MANAGE_USERS_PERMISSION)]
public class Delete : BaseAdminPageModel
{
    public async Task<IActionResult> OnPost(string id)
    {
        var response = await Mediator.Send(new DeleteUser.Command { Id = id });
        if (response.Success)
        {
            SetSuccessMessage($"User has been deleted.");
        }
        else
        {
            SetErrorMessage(response.Error, response.GetErrors());
        }

        return RedirectToPage("/Users/Index"); 
    }
}