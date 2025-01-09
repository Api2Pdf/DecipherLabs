using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using CSharpVitamins;

namespace RaythaZero.Domain.Entities;

public class Project : BaseAuditableEntity
{
    public string? Label { get; set; }
    public bool? IsArchived { get; set; } = false;
    public string? _ProjectData { get; set; }
    
    private dynamic _projectData;

    [NotMapped]
    public dynamic ProjectData 
    { 
        get 
        { 
            if (_projectData == null)
            {
                _projectData = JsonSerializer.Deserialize<dynamic>(_ProjectData ?? JsonSerializer.Serialize(new ProjectLevelInfo()));
            }
            return _projectData; 
        } 
        set
        {
            _projectData = value;
            _ProjectData = JsonSerializer.Serialize(value);
        }
    } 
}

public record ProjectLevelInfo
{
    public string DsipProposalNumber { get; init; } = string.Empty;
    public string TypeOfProposal { get; init; } = string.Empty;
    public ShortGuid? TopLevel { get; init; }
    public ShortGuid? ServiceSpecific { get; init; }
    public ShortGuid? Topic { get; init; }
    public IEnumerable<string> OtherDirectCostSelections { get; init; } = new List<string>();
    public IEnumerable<Resume> Resumes { get; init; } = new List<Resume>();
    public TravelDto Travel { get; init; } = new TravelDto();
    public MaterialsDto Materials { get; init; } = new MaterialsDto();
    public SuppliesDto Supplies { get; init; } = new SuppliesDto();
    public EquipmentDto Equipment { get; init; } = new EquipmentDto();
    public OtherDirectCosts OtherDirectCosts { get; init; } = new OtherDirectCosts();
    public ConsultantDto Consultant { get; init; } = new ConsultantDto();
}

public record Resume
{
    public string FileName { get; init; } = string.Empty;
    public ShortGuid? ResumeMediaId { get; init; } = string.Empty;
}

public abstract record Subtier
{
    public string Description { get; init; } = string.Empty;
    public ShortGuid? DescriptionMediaId { get; init; }
}

public record TravelDto : Subtier
{
    public int NumberOfTrips { get; init; } = 0;
    public int NumberOfTravelers { get; init; } = 0;
    public string LocationOfGovEndUser { get; init; } = string.Empty;
    public string LocationOfSubcontractor { get; init; } = string.Empty;
    public bool UseRideshare { get; init; } = false;
    public bool UseRentalCar { get; init; } = false;
}

public record SubcontractorDto : Subtier
{
    public string Url { get; init; } = string.Empty;
}

public record ConsultantDto : Subtier
{
    public string Url { get; init; } = string.Empty;
}

public record SuppliesDto : Subtier
{
}

public record EquipmentDto : Subtier
{
}

public record OtherDirectCosts : Subtier
{
}

public record MaterialsDto : Subtier
{
}
