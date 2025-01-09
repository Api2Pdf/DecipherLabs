using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CSharpVitamins;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.OrganizationSettings;
using RaythaZero.Application.OrganizationSettings.Queries;

namespace RaythaZero.Web.Areas.Public.Pages.Company;

public class Index : BasePublicPageModel
{
    [BindProperty]
    public FormModel Form { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        var response = await Mediator.Send(new GetOrganizationSettings.Query());
        CompanyLevelInfoDto companyLevelInfoDto =
            JsonSerializer.Deserialize<CompanyLevelInfoDto>(
                JsonSerializer.Serialize(response.Result.CompanyLevelInfo));
        
        Form = new FormModel
        {
            LegalName = companyLevelInfoDto.LegalName,
            Url = companyLevelInfoDto.Url,
            CityAndStateHq = companyLevelInfoDto.CityAndStateHq,
            OffersBenefits = companyLevelInfoDto.OffersBenefits,
            OffersBenefitsDescription = companyLevelInfoDto.OffersBenefitsDescription,
            WageRateSheetMediaId = companyLevelInfoDto.WageRateSheetMediaId,
            PreviousCostVolumeExcelMediaId = companyLevelInfoDto.PreviousCostVolumeExcelMediaId,
            PreviousCostVolumeWordMediaId = companyLevelInfoDto.PreviousCostVolumeWordMediaId,
            FinancialStatements =  companyLevelInfoDto.FinancialStatements
        };
        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        var response = await Mediator.Send(new RaythaZero.Application.OrganizationSettings.Commands.EditCompanylevelInfo.Command()
        { 
            LegalName = Form.LegalName,
            Url = Form.Url,
            CityAndStateHq = Form.CityAndStateHq,
            OffersBenefits = Form.OffersBenefits,
            OffersBenefitsDescription = Form.OffersBenefitsDescription,
            WageRateSheetMediaId = Form.WageRateSheetMediaId,
            PreviousCostVolumeExcelMediaId = Form.PreviousCostVolumeExcelMediaId,
            PreviousCostVolumeWordMediaId = Form.PreviousCostVolumeWordMediaId,
            FinancialStatements = Form.FinancialStatements
        });

        if (response.Success)
        {
            SetSuccessMessage("Company info saved successfully.");
            return RedirectToPage("/Projects/Index", new { id = response.Result });
        }
        else
        {
            SetErrorMessage(response.GetErrors());
            return Page();
        } 
    }
    
    public record FormModel 
    {
        [Display(Name = "Legal name")]
        public string LegalName { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public string CityAndStateHq { get; init; } = string.Empty;
        public bool OffersBenefits { get; init; } = false;
        public string OffersBenefitsDescription { get; init; } = string.Empty;
        public ShortGuid? WageRateSheetMediaId { get; init; }
        public ShortGuid? PreviousCostVolumeExcelMediaId { get; init; }
        public ShortGuid? PreviousCostVolumeWordMediaId { get; init; }
        public IEnumerable<ShortGuid> FinancialStatements { get; init; } =  new List<ShortGuid>();
    }
}