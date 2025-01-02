using Microsoft.AspNetCore.Mvc;

namespace RaythaZero.Web.Areas.Public.Pages.Projects;

public class Create : BasePublicPageModel
{
    [BindProperty]
    public FormModel Form { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        Form = new FormModel();
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        return Page();
    }

    public record FormModel
    {
        public string Label { get; set; }
    }
}