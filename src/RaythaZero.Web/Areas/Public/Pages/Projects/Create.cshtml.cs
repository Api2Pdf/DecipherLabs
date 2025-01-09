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
        var response = await Mediator.Send(new RaythaZero.Application.Projects.Commands.CreateProject.Command 
        { 
            Label = Form.Label
        });

        if (response.Success)
        {
            SetSuccessMessage("Project created successfully.");
            return RedirectToPage("/Projects/Manage", new { id = response.Result });
        }
        else
        {
            SetErrorMessage(response.GetErrors());
            return Page();
        } 
    }

    public record FormModel
    {
        public string Label { get; set; }
    }
}