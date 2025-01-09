using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using CSharpVitamins;

namespace RaythaZero.Domain.Entities;

public class OrganizationSettings : BaseEntity
{
    public string? OrganizationName { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? TimeZone { get; set; }
    public string? DateFormat { get; set; }
    public bool SmtpOverrideSystem { get; set; } = false;
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SmtpDefaultFromAddress { get; set; }
    public string? SmtpDefaultFromName { get; set; }
    
    public string? _CompanySetupData { get; set; }
    
    private dynamic _companySetupData;

    [NotMapped]
    public dynamic CompanySetupData 
    { 
        get 
        { 
            _companySetupData = JsonSerializer.Deserialize<dynamic>(_CompanySetupData ?? JsonSerializer.Serialize(new CompanyLevelInfo()));
            return _companySetupData; 
        } 
        set
        {
            _companySetupData = value;
            _CompanySetupData = JsonSerializer.Serialize(value);
        }
    } 
}

public record CompanyLevelInfo
{
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
