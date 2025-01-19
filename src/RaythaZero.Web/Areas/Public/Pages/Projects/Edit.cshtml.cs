using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RaythaZero.Application.Projects.Queries;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Web.Areas.Public.Pages.Projects;

public class Edit : BasePublicPageModel
{
    [BindProperty] public FormModel Form { get; set; }

    
    public async Task<IActionResult> OnGet(string id)
    {
        var response = await Mediator.Send(new GetProjectById.Query { Id = id });
        var otherDirectCostSelections = new List<SelectListItem>();
        foreach (var option in DirectCostOptions)
        {
            var isSelected = response.Result.ProjectData.OtherDirectCostSelections.Contains(option);
            otherDirectCostSelections.Add(new SelectListItem { Text = option, Value = option, Selected = isSelected });
        }
        
        Form = new FormModel
        {
            Id = response.Result.Id,
            Label = response.Result.Label,
            DsipProposalNumber = response.Result.ProjectData.DsipProposalNumber,
            TypeOfProposal = response.Result.ProjectData.TypeOfProposal,
            OtherDirectCostSelections = otherDirectCostSelections,
            TopicNumber = response.Result.ProjectData.TopicNumber
        };
        ProjectId = id;
        ProjectName = response.Result.Label;
        ProjectIsArchived = response.Result.IsArchived.Value;
        return Page();
    }

    public async Task<IActionResult> OnPost(string id)
    {
        var response = await Mediator.Send(new RaythaZero.Application.Projects.Commands.EditProject.Command 
        { 
            Id = Form.Id,
            Label = Form.Label,
            DsipProposalNumber = Form.DsipProposalNumber,
            TypeOfProposal = Form.TypeOfProposal,
            TopicNumber = Form.TopicNumber,
            OtherDirectCostSelections = Form.OtherDirectCostSelections.Where(p => p.Selected).Select(x => x.Value)
        });

        if (response.Success)
        {
            SetSuccessMessage("Project edited successfully.");
            return RedirectToPage("/Projects/Manage", new { id = response.Result });
        }
        else
        {
            SetErrorMessage(response.GetErrors());
            ProjectId = id;
            ProjectName = Form.Label;
            return Page();
        } 
    }
    
    public IEnumerable<Topic> Topics 
    {
        get
        {
            return Application.Projects.Utils.ProjectUtils.GetTopics();
        }
    }
    
    public IEnumerable<string> DirectCostOptions
    {
        get
        {
            return new[] { "Materials", "Supplies", "Equipment", "Consultants", "Travel", "Subcontractors", "Other Direct Costs" };
        }
    }
    
    public record FormModel
    {
        public string Id { get; set; }
        public string Label { get; set; }
        [Display(Name = "DSIP Proposal number")]
        public string DsipProposalNumber { get; set; } = string.Empty;
        
        [Display(Name = "Type of proposal")]
        public string TypeOfProposal { get; set; } = string.Empty;
        
        [Display(Name = "Other direct costs")]
        public List<SelectListItem> OtherDirectCostSelections { get; set; } = new List<SelectListItem>();
        
        [Display(Name = "Topic solicitation")]
        public string TopicNumber { get; set; } = string.Empty;
    }
    
    public record UploadRequest 
    {
        public IEnumerable<string> UploadedIds { get; set; } = new List<string>();
    }
}