using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.Projects.Commands;
using RaythaZero.Application.Projects.Queries;
using RaythaZero.Web.Areas.Public.Pages.Projects._Partials;

namespace RaythaZero.Web.Areas.Public.Pages.Projects;
public class Manage : BasePublicPageModel
{
    [BindProperty]
    public FormModel Form { get; set; }
    public string DsipProposalNumber { get; set; } = string.Empty;
    public string TypeOfProposal { get; set; } = string.Empty;
    public IEnumerable<string> OtherDirectCostSelections { get; set; } = new List<string>();

    public int CurrentNumberOfFiles { get; set; } = 0;

    public List<string> UploadedIds { get; set; } = new List<string>();
    public async Task<IActionResult> OnGet(string id)
    {
        var response = await Mediator.Send(new GetProjectById.Query { Id = id });
        DsipProposalNumber = response.Result.ProjectData.DsipProposalNumber;
        TypeOfProposal = response.Result.ProjectData.TypeOfProposal;
        OtherDirectCostSelections = response.Result.ProjectData.OtherDirectCostSelections;
        
        Form = new FormModel
        {
            Id = response.Result.Id,
            Resumes = response.Result.ProjectData.Resumes.ToList(),
            Travel = new TravelViewModel
            {
                NumberOfTrips = response.Result.ProjectData.Travel.NumberOfTrips,
                NumberOfTravelers = response.Result.ProjectData.Travel.NumberOfTravelers,
                UseRideshare = response.Result.ProjectData.Travel.UseRideshare,
                UseRentalCar = response.Result.ProjectData.Travel.UseRentalCar,
                LocationOfSubcontractor = response.Result.ProjectData.Travel.LocationOfSubcontractor,
                LocationOfGovEndUser = response.Result.ProjectData.Travel.LocationOfGovEndUser,
                Description = response.Result.ProjectData.Travel.Description,
                DescriptionMediaId = response.Result.ProjectData.Travel.DescriptionMediaId
            },
           Subcontractor = new SubcontractorViewModel
           {
               Url = response.Result.ProjectData.Subcontractor.Url,
               Description = response.Result.ProjectData.Subcontractor.Description,
               DescriptionMediaId = response.Result.ProjectData.Subcontractor.DescriptionMediaId
           },
           Materials = new MaterialsViewModel
           {
               Description = response.Result.ProjectData.Materials.Description,
               DescriptionMediaId = response.Result.ProjectData.Materials.DescriptionMediaId
           },
           Equipment = new EquipmentViewModel
           {
               Description = response.Result.ProjectData.Equipment.Description,
               DescriptionMediaId = response.Result.ProjectData.Equipment.DescriptionMediaId
           },
           OtherDirectCosts = new OtherDirectCostsViewModel
           {
               Description = response.Result.ProjectData.OtherDirectCosts.Description,
               DescriptionMediaId = response.Result.ProjectData.OtherDirectCosts.DescriptionMediaId
           },
           Supplies = new SuppliesViewModel
           {
               Description = response.Result.ProjectData.Supplies.Description,
               DescriptionMediaId = response.Result.ProjectData.Supplies.DescriptionMediaId
           },
           Consultant = new ConsultantViewModel
           {
               Description = response.Result.ProjectData.Consultant.Description,
               DescriptionMediaId = response.Result.ProjectData.Consultant.DescriptionMediaId,
               Url = response.Result.ProjectData.Consultant.Url
           }
        };
        ProjectId = id;
        ProjectName = response.Result.Label;
        ProjectIsArchived = response.Result.IsArchived.Value;
        return Page();
    }

    public async Task<IActionResult> OnPost(string id)
    {
        var response = await Mediator.Send(new ManageProject.Command
        {
            Id = id,
            Resumes = Form.Resumes,
            TravelDescription = Form.Travel.Description,
            TravelDescriptionMediaId = Form.Travel.DescriptionMediaId,
            NumberOfTravelers = Form.Travel.NumberOfTravelers,
            NumberOfTrips = Form.Travel.NumberOfTrips,
            UseRideshare = Form.Travel.UseRideshare,
            UseRentalCar = Form.Travel.UseRentalCar,
            LocationOfSubcontractor = Form.Travel.LocationOfSubcontractor,
            LocationOfGovEndUser = Form.Travel.LocationOfGovEndUser,
            SuppliesDescription = Form.Supplies.Description,
            SuppliesDescriptionMediaId = Form.Supplies.DescriptionMediaId,
            MaterialsDescription = Form.Materials.Description,
            MaterialsDescriptionMediaId = Form.Materials.DescriptionMediaId,
            EquipmentDescription = Form.Equipment.Description,
            EquipmentDescriptionMediaId = Form.Equipment.DescriptionMediaId,
            OtherDirectCostsDescription = Form.OtherDirectCosts.Description,
            OtherDirectCostsDescriptionMediaId = Form.OtherDirectCosts.DescriptionMediaId,
            SubcontractorsUrl = Form.Subcontractor.Url,
            SubcontractorsDescription = Form.Subcontractor.Description,
            SubcontractorsDescriptionMediaId = Form.Subcontractor.DescriptionMediaId,
            ConsultantsUrl = Form.Consultant.Url,
            ConsultantsDescription = Form.Consultant.Description,
            ConsultantsDescriptionMediaId = Form.Consultant.DescriptionMediaId
        });

        if (response.Success)
        {
            SetSuccessMessage("Changes saved successfully.");
            return RedirectToPage("/Projects/Manage", new { id = id });
        }
        else
        {
            var result = await Mediator.Send(new GetProjectById.Query { Id = id });
            DsipProposalNumber = result.Result.ProjectData.DsipProposalNumber;
            TypeOfProposal = result.Result.ProjectData.TypeOfProposal;
            OtherDirectCostSelections = result.Result.ProjectData.OtherDirectCostSelections;
            SetErrorMessage(response.GetErrors());
            return Page();
        }
    }
    
    public async Task<IActionResult> OnPostProcessResumesUpload(string id, [FromBody] UploadRequest uploads)
    {
        CurrentNumberOfFiles = uploads.CurrentNumberOfFiles;
        UploadedIds = uploads.UploadedIds.ToList();
        return new PartialViewResult { ViewName = "_Partials/ProcessResumesUpload", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnPostProcessTravelDescriptionUpload(string id, [FromBody] UploadRequest uploads)
    {
        ProjectId = id;
        Form = new FormModel();
        Form.Travel.DescriptionMediaId = uploads.UploadedIds.FirstOrDefault();
        return new PartialViewResult { ViewName = "_Partials/ProcessTravelDescriptionUpload", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnGetDeleteTravelDescriptionUpload(string id)
    {
        ProjectId = id;
        Form = new FormModel();
        return new PartialViewResult { ViewName = "_Partials/TravelDescriptionUpload", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnPostProcessSuppliesDescriptionUpload(string id, [FromBody] UploadRequest uploads)
    {
        ProjectId = id;
        Form = new FormModel();
        Form.Supplies.DescriptionMediaId = uploads.UploadedIds.FirstOrDefault();
        return new PartialViewResult { ViewName = "_Partials/ProcessSuppliesDescriptionUpload", ViewData=ViewData };
    }
    public async Task<IActionResult> OnGetDeleteSuppliesDescriptionUpload(string id)
    {
        ProjectId = id;
        Form = new FormModel();
        return new PartialViewResult { ViewName = "_Partials/SuppliesDescriptionUpload", ViewData=ViewData };
    }

    public async Task<IActionResult> OnPostProcessSubcontractorsDescriptionUpload(string id, [FromBody] UploadRequest uploads)
    {
        ProjectId = id;
        Form = new FormModel();
        Form.Subcontractor.DescriptionMediaId = uploads.UploadedIds.FirstOrDefault();
        return new PartialViewResult { ViewName = "_Partials/ProcessSubcontractorsDescriptionUpload", ViewData=ViewData };
    }
    public async Task<IActionResult> OnGetDeleteSubcontractorsDescriptionUpload(string id)
    {
        ProjectId = id;
        Form = new FormModel();
        return new PartialViewResult { ViewName = "_Partials/SubcontractorsDescriptionUpload", ViewData=ViewData };
    }
    public async Task<IActionResult> OnPostProcessConsultantsDescriptionUpload(string id, [FromBody] UploadRequest uploads)
    {
        ProjectId = id;
        Form = new FormModel();
        Form.Consultant.DescriptionMediaId = uploads.UploadedIds.FirstOrDefault();
        return new PartialViewResult { ViewName = "_Partials/ProcessConsultantsDescriptionUpload", ViewData=ViewData };
    }
    public async Task<IActionResult> OnGetDeleteConsultantsDescriptionUpload(string id)
    {
        ProjectId = id;
        Form = new FormModel();
        return new PartialViewResult { ViewName = "_Partials/ConsultantsDescriptionUpload", ViewData=ViewData };
    }
    public async Task<IActionResult> OnPostProcessMaterialsDescriptionUpload(string id, [FromBody] UploadRequest uploads)
    {
        ProjectId = id;
        Form = new FormModel();
        Form.Materials.DescriptionMediaId = uploads.UploadedIds.FirstOrDefault();
        return new PartialViewResult { ViewName = "_Partials/ProcessMaterialsDescriptionUpload", ViewData=ViewData };
    }
    public async Task<IActionResult> OnGetDeleteMaterialsDescriptionUpload(string id)
    {
        ProjectId = id;
        Form = new FormModel();
        return new PartialViewResult { ViewName = "_Partials/MaterialsDescriptionUpload", ViewData=ViewData };
    }
    public async Task<IActionResult> OnPostProcessEquipmentDescriptionUpload(string id, [FromBody] UploadRequest uploads)
    {
        ProjectId = id;
        Form = new FormModel();
        Form.Equipment.DescriptionMediaId = uploads.UploadedIds.FirstOrDefault();
        return new PartialViewResult { ViewName = "_Partials/ProcessEquipmentDescriptionUpload", ViewData=ViewData };
    }
    public async Task<IActionResult> OnGetDeleteEquipmentDescriptionUpload(string id)
    {
        ProjectId = id;
        Form = new FormModel();
        return new PartialViewResult { ViewName = "_Partials/EquipmentDescriptionUpload", ViewData=ViewData };
    }
    public async Task<IActionResult> OnPostProcessOtherDirectCostsDescriptionUpload(string id, [FromBody] UploadRequest uploads)
    {
        ProjectId = id;
        Form = new FormModel();
        Form.OtherDirectCosts.DescriptionMediaId = uploads.UploadedIds.FirstOrDefault();
        return new PartialViewResult { ViewName = "_Partials/ProcessOtherDirectCostsDescriptionUpload", ViewData=ViewData };
    }
    public async Task<IActionResult> OnGetDeleteOtherDirectCostsDescriptionUpload(string id)
    {
        ProjectId = id;
        Form = new FormModel();
        return new PartialViewResult { ViewName = "_Partials/OtherDirectCostsDescriptionUpload", ViewData=ViewData };
    }
    public record FormModel
    {
        public string Id { get; set; }
        public List<string> Resumes { get; set; } = new List<string>();
        public TravelViewModel Travel { get; set; } = new TravelViewModel();
        public MaterialsViewModel Materials { get; set; } = new MaterialsViewModel();
        public SuppliesViewModel Supplies { get; set; } = new SuppliesViewModel();
        public EquipmentViewModel Equipment { get; set; } = new EquipmentViewModel();
        public OtherDirectCostsViewModel OtherDirectCosts { get; set; } = new OtherDirectCostsViewModel();
        public ConsultantViewModel Consultant { get; set; } = new ConsultantViewModel();
        public SubcontractorViewModel Subcontractor { get; set; } = new SubcontractorViewModel();
    }
    public abstract record AbstractSubtierViewModel
    {
        public string Description { get; set; } = string.Empty;
        public string DescriptionMediaId { get; set; }
    }

    public record TravelViewModel : AbstractSubtierViewModel
    {
        [Display(Name = "Number of trips")]
        public int NumberOfTrips { get; set; } = 0;
        [Display(Name = "Number of travelers")]
        public int NumberOfTravelers { get; set; } = 0;
        [Display(Name = "Location of gov end user")]
        public string LocationOfGovEndUser { get; set; } = string.Empty;
        [Display(Name = "Location of subcontractor")]
        public string LocationOfSubcontractor { get; set; } = string.Empty;
        [Display(Name = "Use rideshare")]
        public bool UseRideshare { get; set; } = false;
        [Display(Name = "Use rental car")]
        public bool UseRentalCar { get; set; } = false;
    }

    public record SubcontractorViewModel : AbstractSubtierViewModel
    {
        public string Url { get; set; } = string.Empty;
    }

    public record ConsultantViewModel : AbstractSubtierViewModel
    {
        public string Url { get; set; } = string.Empty;
    }

    public record SuppliesViewModel : AbstractSubtierViewModel
    {
    }

    public record EquipmentViewModel : AbstractSubtierViewModel
    {
    }

    public record OtherDirectCostsViewModel : AbstractSubtierViewModel
    {
    }

    public record MaterialsViewModel : AbstractSubtierViewModel
    {
    }

    public record UploadRequest 
    {
        public IEnumerable<string> UploadedIds { get; set; } = new List<string>();
        public int CurrentNumberOfFiles { get; set; } = 0;
    }
}