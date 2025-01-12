using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using CSharpVitamins;
using RaythaZero.Domain.JsonConverters;

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
    
    private CompanyLevelInfo _companySetupData;

    [NotMapped]
    public CompanyLevelInfo CompanySetupData 
    { 
        get
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new ShortGuidConverter() }
            };
            string defaultCompanySetupData = JsonSerializer.Serialize(new CompanyLevelInfo(), options);
            _companySetupData = JsonSerializer.Deserialize<CompanyLevelInfo>(_CompanySetupData ?? defaultCompanySetupData, options);
            return _companySetupData; 
        } 
        set
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Converters = { new ShortGuidConverter() }
            };
            _companySetupData = value;
            _CompanySetupData = JsonSerializer.Serialize(value, options);
        }
    } 
}

public record CompanyLevelInfo
{
    public string LegalName { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string CityHq { get; init; } = string.Empty;
    public string StateHq { get; init; } = string.Empty;
    public bool OffersBenefits { get; init; } = false;
    public string OffersBenefitsDescription { get; init; } = string.Empty;
    public string WageRateSheetMediaId { get; init; }
    public string PreviousCostVolumeExcelMediaId { get; init; }
    public string PreviousCostVolumeWordMediaId { get; init; }
    public IEnumerable<string> FinancialStatements { get; init; } =  new List<string>();
}
