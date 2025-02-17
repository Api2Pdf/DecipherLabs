using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.Projects.Commands;

namespace RaythaZero.Web.Areas.Public.Pages.Projects;

public class Archive : BasePublicPageModel
{
    public async Task<IActionResult> OnPost(string id)
    {
        var response = await Mediator.Send(new ArchiveProject.Command { Id = id });
        if (response.Success)
        {
            SetSuccessMessage("Project successfully archived");
        }
        else
        {
            SetErrorMessage(response.GetErrors());
        }
        return RedirectToPage("/Projects/Index"); 
    }
}