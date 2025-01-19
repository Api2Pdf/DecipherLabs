using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.Projects.Commands;

namespace RaythaZero.Web.Areas.Public.Pages.Projects;

public class Restore : BasePublicPageModel
{
    public async Task<IActionResult> OnPost(string id)
    {
        var response = await Mediator.Send(new ArchiveProject.Command { Id = id, IsArchived = false});
        if (response.Success)
        {
            SetSuccessMessage("Project successfully restored");
        }
        else
        {
            SetErrorMessage(response.GetErrors());
        }
        return RedirectToPage("/Projects/Index"); 
    }
}