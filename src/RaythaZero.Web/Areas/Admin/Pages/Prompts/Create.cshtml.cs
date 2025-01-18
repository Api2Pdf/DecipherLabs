using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.Prompts.Commands;

namespace RaythaZero.Web.Areas.Admin.Pages.Prompts;

public class Create : BaseAdminPageModel
{
    [BindProperty]
    public FormModel Form { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        var input = new CreatePrompt.Command
        {
            Label = Form.Label,
            DeveloperName = Form.DeveloperName,
            ResultType = Form.ResultType,
            PromptText = Form.PromptText,
        };
        
        var response = await Mediator.Send(input);
        if (response.Success)
        {
            SetSuccessMessage("Successfully created a prompt.");
            return RedirectToPage("Index");
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

        [Display(Name = "Developer name")]
        public string DeveloperName { get; set; }

        [Display(Name = "Prompt text")]
        public string PromptText { get; set; }

        [Display(Name = "Result type")]
        public string ResultType { get; set; }
    }
}