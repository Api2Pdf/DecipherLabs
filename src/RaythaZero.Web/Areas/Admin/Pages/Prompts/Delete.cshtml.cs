using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.Prompts.Commands;

namespace RaythaZero.Web.Areas.Admin.Pages.Prompts;

public class Delete : BaseAdminPageModel
{
    public async Task<IActionResult> OnPost(string id)
    {
        var response = await Mediator.Send(new DeletePrompt.Command { Id = id });
        if (response.Success)
        {
            SetSuccessMessage("Successfully deleted prompt.");
        }
        else
        {
            SetErrorMessage("Error deleting prompt.");
        }
        return RedirectToPage("/Prompts/Index");
    }
}