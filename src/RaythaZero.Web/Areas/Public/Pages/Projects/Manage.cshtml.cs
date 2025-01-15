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
        public int NumberOfTrips { get; set; } = 0;
        public int NumberOfTravelers { get; set; } = 0;
        public string LocationOfGovEndUser { get; set; } = string.Empty;
        public string LocationOfSubcontractor { get; set; } = string.Empty;
        public bool UseRideshare { get; set; } = false;
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