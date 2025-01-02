using Microsoft.AspNetCore.Mvc;

namespace RaythaZero.Web.Areas.Public.Pages.Projects;

public class Archive : BasePublicPageModel
{
    public async Task<IActionResult> OnPost(string id)
    {
        return RedirectToPage("/Projects/Index"); 
    }
}