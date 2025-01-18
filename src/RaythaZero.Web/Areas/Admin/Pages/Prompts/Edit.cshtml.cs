using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.Prompts.Commands;
using RaythaZero.Application.Prompts.Queries;

namespace RaythaZero.Web.Areas.Admin.Pages.Prompts;

public class Edit : BaseAdminPageModel, ISubActionViewModel 
{
    public string Id { get; set; }
    
    [BindProperty]
    public FormModel Form { get; set;  }
    
    public async Task<IActionResult> OnGet(string id)
    {
        var response = await Mediator.Send(new GetPromptById.Query { Id = id });

        Form = new FormModel
        {
            Id = id,
            Label = response.Result.Label,
            DeveloperName = response.Result.DeveloperName,
            IsActive = response.Result.IsActive,
            PromptText = response.Result.PromptText,
            ResultType = response.Result.ResultType
        };
        Id = id;
        return Page();
    }

    public async Task<IActionResult> OnPost(string id)
    {
        Id = id;
        var input = new EditPrompt.Command
        {
            Id = id,
            Label = Form.Label,
            PromptText = Form.PromptText,
            ResultType = Form.ResultType
        };
        var response = await Mediator.Send(input);

        if (response.Success)
        {
            SetSuccessMessage("Prompt updated successfully");
            return RedirectToPage("/Prompts/Edit", new { id = id });
        }
        else
        {
            SetErrorMessage(response.GetErrors());
            return Page();
        }
    }
    
    public record FormModel 
    {
        public string Id { get; set; }
        
        public string Label { get; set; }

        [Display(Name = "Developer name")]
        public string DeveloperName { get; set; }

        [Display(Name = "Prompt text")]
        public string PromptText { get; set; }

        [Display(Name = "Is active")] 
        public bool IsActive { get; set; } = true;

        [Display(Name = "Result type")]
        public string ResultType { get; set; }
    }
}