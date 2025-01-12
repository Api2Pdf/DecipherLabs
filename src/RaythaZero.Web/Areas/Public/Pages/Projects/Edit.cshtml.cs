using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RaythaZero.Application.Projects.Queries;

namespace RaythaZero.Web.Areas.Public.Pages.Projects;

public class Edit : BasePublicPageModel
{
    [BindProperty]
    public FormModel Form { get; set; }
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
            TopicMediaId = response.Result.ProjectData.TopicMediaId,
            TopLevelMediaId = response.Result.ProjectData.TopLevelMediaId,
            ServiceSpecificMediaId = response.Result.ProjectData.ServiceSpecificMediaId,
            OtherDirectCostSelections = otherDirectCostSelections
        };
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
            TopicMediaId = Form.TopicMediaId,
            ServiceSpecificMediaId = Form.ServiceSpecificMediaId,
            TopLevelMediaId = Form.TopLevelMediaId,
            OtherDirectCostSelections = Form.OtherDirectCostSelections.Where(p => p.Selected).Select(x => x.Value)
        });

        if (response.Success)
        {
            SetSuccessMessage("Project edited successfully.");
            return RedirectToPage("/Projects/Edit", new { id = response.Result });
        }
        else
        {
            SetErrorMessage(response.GetErrors());
            return Page();
        } 
    }
    
    public async Task<IActionResult> OnPostProcessTopLevelUpload(string id, [FromBody] UploadRequest uploads)
    {
        Form = new FormModel
        {
            Id = id,
            TopLevelMediaId = uploads.UploadedIds.First()
        };
        return new PartialViewResult { ViewName = "_Partials/ProcessTopLevelUpload", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnGetDeleteTopLevelUpload(string id)
    {
        Form = new FormModel
        {
            Id = id,
        };
        return new PartialViewResult { ViewName = "_Partials/TopLevelUpload", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnPostProcessServiceSpecificUpload(string id, [FromBody] UploadRequest uploads)
    {
        Form = new FormModel
        {
            Id = id,
            ServiceSpecificMediaId = uploads.UploadedIds.First()
        };
        return new PartialViewResult { ViewName = "_Partials/ProcessServiceSpecificUpload", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnGetDeleteServiceSpecificUpload(string id)
    {
        Form = new FormModel
        {
            Id = id,
        };
        return new PartialViewResult { ViewName = "_Partials/ServiceSpecificUpload", ViewData=ViewData };
    }
    public async Task<IActionResult> OnPostProcessTopicUpload(string id, [FromBody] UploadRequest uploads)
    {
        Form = new FormModel
        {
            Id = id,
            TopicMediaId = uploads.UploadedIds.First()
        };
        return new PartialViewResult { ViewName = "_Partials/ProcessTopicUpload", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnGetDeleteTopicUpload(string id)
    {
        Form = new FormModel
        {
            Id = id,
        };
        return new PartialViewResult { ViewName = "_Partials/TopicUpload", ViewData=ViewData };
    }
    public IEnumerable<string> DirectCostOptions
    {
        get
        {
            return new[] { "Materials", "Supplies", "Equipment", "Consultants", "Travel", "Other Direct Costs" };
        }
    }
    
    public record FormModel
    {
        public string Id { get; set; }
        public string Label { get; set; }
        [Display(Name = "DSIP Proposal number")]
        public string DsipProposalNumber { get; init; } = string.Empty;
        
        [Display(Name = "Type of proposal")]
        public string TypeOfProposal { get; init; } = string.Empty;
        
        [Display(Name = "Top level DoD BAA")]
        public string TopLevelMediaId { get; init; }
        
        [Display(Name = "Service specific solicitation")]
        public string ServiceSpecificMediaId { get; init; }
        
        [Display(Name = "Specific topic solicitation")]
        public string TopicMediaId { get; init; }
        
        [Display(Name = "Other direct costs")]
        public List<SelectListItem> OtherDirectCostSelections { get; init; } = new List<SelectListItem>();

    }
    
    public record UploadRequest 
    {
        public IEnumerable<string> UploadedIds { get; set; } = new List<string>();
    }
}