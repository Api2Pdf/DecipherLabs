using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.OrganizationSettings.Queries;

namespace RaythaZero.Web.Areas.Public.Pages.Company;

public class Index : BasePublicPageModel
{
    [BindProperty]
    public FormModel Form { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        var response = await Mediator.Send(new GetOrganizationSettings.Query());
        
        Form = new FormModel
        {
            LegalName = response.Result.CompanyLevelInfo.LegalName,
            Url = response.Result.CompanyLevelInfo.Url,
            StateHq = response.Result.CompanyLevelInfo.StateHq,
            CityHq = response.Result.CompanyLevelInfo.CityHq,
            OffersBenefits = response.Result.CompanyLevelInfo.OffersBenefits,
            OffersBenefitsDescription = response.Result.CompanyLevelInfo.OffersBenefitsDescription,
            WageRateSheetMediaId = response.Result.CompanyLevelInfo.WageRateSheetMediaId,
            PreviousCostVolumeExcelMediaId = response.Result.CompanyLevelInfo.PreviousCostVolumeExcelMediaId,
            PreviousCostVolumeWordMediaId = response.Result.CompanyLevelInfo.PreviousCostVolumeWordMediaId,
            BalanceSheetMediaId = response.Result.CompanyLevelInfo.BalanceSheetMediaId,
            ProfitAndLossMediaId = response.Result.CompanyLevelInfo.ProfitAndLossMediaId
        };
        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        var response = await Mediator.Send(new RaythaZero.Application.OrganizationSettings.Commands.EditCompanylevelInfo.Command()
        { 
            LegalName = Form.LegalName,
            Url = Form.Url,
            CityHq = Form.CityHq,
            StateHq = Form.StateHq,
            OffersBenefits = Form.OffersBenefits,
            OffersBenefitsDescription = Form.OffersBenefitsDescription,
            WageRateSheetMediaId = Form.WageRateSheetMediaId,
            PreviousCostVolumeExcelMediaId = Form.PreviousCostVolumeExcelMediaId,
            PreviousCostVolumeWordMediaId = Form.PreviousCostVolumeWordMediaId,
            BalanceSheetMediaId = Form.BalanceSheetMediaId,
            ProfitAndLossMediaId = Form.ProfitAndLossMediaId,
        });

        if (response.Success)
        {
            SetSuccessMessage("Company info saved successfully.");
            return RedirectToPage("/Company/Index", new { id = response.Result });
        }
        else
        {
            SetErrorMessage(response.GetErrors());
            return Page();
        } 
    }
    
    public async Task<IActionResult> OnPostProcessWageRatesUpload([FromBody] UploadRequest uploads)
    {
        Form = new FormModel
        {
            WageRateSheetMediaId = uploads.UploadedIds.First()
        };
        return new PartialViewResult { ViewName = "_Partials/ProcessWageRatesUpload", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnGetDeleteWageRatesUpload()
    {
        Form = new FormModel();
        return new PartialViewResult { ViewName = "_Partials/WageRatesUpload", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnPostProcessPreviousCostVolumeExcelUpload([FromBody] UploadRequest uploads)
    {
        Form = new FormModel
        {
            PreviousCostVolumeExcelMediaId = uploads.UploadedIds.First()
        };
        return new PartialViewResult { ViewName = "_Partials/ProcessPreviousCostVolumeExcelUpload", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnGetDeletePreviousCostVolumeExcelUpload()
    {
        Form = new FormModel();
        return new PartialViewResult { ViewName = "_Partials/PreviousCostVolumeUploadExcel", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnPostProcessPreviousCostVolumeWordUpload([FromBody] UploadRequest uploads)
    {
        Form = new FormModel
        {
            PreviousCostVolumeWordMediaId = uploads.UploadedIds.First()
        };
        return new PartialViewResult { ViewName = "_Partials/ProcessPreviousCostVolumeWordUpload", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnGetDeletePreviousCostVolumeWordUpload()
    {
        Form = new FormModel();
        return new PartialViewResult { ViewName = "_Partials/PreviousCostVolumeUploadWord", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnPostProcessBalanceSheetUpload([FromBody] UploadRequest uploads)
    {
        Form = new FormModel
        {
            BalanceSheetMediaId = uploads.UploadedIds.First()
        };
        return new PartialViewResult { ViewName = "_Partials/ProcessBalanceSheetUpload", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnGetDeleteBalanceSheetUpload()
    {
        Form = new FormModel();
        return new PartialViewResult { ViewName = "_Partials/BalanceSheetUpload", ViewData=ViewData };
    }
    public async Task<IActionResult> OnPostProcessProfitAndLossUpload([FromBody] UploadRequest uploads)
    {
        Form = new FormModel
        {
            ProfitAndLossMediaId = uploads.UploadedIds.First()
        };
        return new PartialViewResult { ViewName = "_Partials/ProcessProfitAndLossUpload", ViewData=ViewData };
    }
    
    public async Task<IActionResult> OnGetDeleteProfitAndLossUpload()
    {
        Form = new FormModel();
        return new PartialViewResult { ViewName = "_Partials/ProfitAndLossUpload", ViewData=ViewData };
    }
    public record FormModel 
    {
        [Display(Name = "Legal name")]
        public string LegalName { get; set; } = string.Empty;
        [Display(Name = "Company URL")]
        public string Url { get; set; } = string.Empty;
        [Display(Name = "City HQ")]
        public string CityHq { get; set; } = string.Empty;
        [Display(Name = "State HQ")]
        public string StateHq { get; set; } = string.Empty;
        [Display(Name = "Offers benefits")] 
        public bool OffersBenefits { get; set; } = false;
        [Display(Name = "Description of benefits")]
        public string OffersBenefitsDescription { get; set; } = string.Empty;
        [Display(Name = "Wage rate sheet")]
        public string WageRateSheetMediaId { get; set; } = string.Empty;
        [Display(Name = "Previous cost volume excel file")]
        public string PreviousCostVolumeExcelMediaId { get; set; } = string.Empty;
        [Display(Name = "Previous cost volume word file")]
        public string PreviousCostVolumeWordMediaId { get; set; } = string.Empty;
        [Display(Name = "Balance sheet")]
        public string BalanceSheetMediaId { get; set; } = string.Empty;
        [Display(Name = "Profit and loss")]
        public string ProfitAndLossMediaId { get; set; } = string.Empty;
    }

    public record UploadRequest 
    {
        public IEnumerable<string> UploadedIds { get; set; } = new List<string>();
    }
}