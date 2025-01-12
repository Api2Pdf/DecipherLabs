using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using CSharpVitamins;

namespace RaythaZero.Domain.Entities;

public class Project : BaseAuditableEntity
{
    public string? Label { get; set; }
    public bool? IsArchived { get; set; } = false;
    public string? _ProjectData { get; set; }
    
    private ProjectLevelInfo _projectData;

    [NotMapped]
    public ProjectLevelInfo ProjectData 
    { 
        get 
        { 
            if (_projectData == null)
            {
                _projectData = JsonSerializer.Deserialize<ProjectLevelInfo>(_ProjectData ?? JsonSerializer.Serialize(new ProjectLevelInfo()));
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
    public string DsipProposalNumber { get; set; } = string.Empty;
    public string TypeOfProposal { get; set; } = string.Empty;
    public string TopLevelMediaId { get; set; }
    public string ServiceSpecificMediaId { get; set; }
    public string TopicMediaId { get; set; }
    public IEnumerable<string> OtherDirectCostSelections { get; set; } = new List<string>();
    public IEnumerable<Resume> Resumes { get; set; } = new List<Resume>();
    public TravelDto Travel { get; set; } = new TravelDto();
    public MaterialsDto Materials { get; set; } = new MaterialsDto();
    public SuppliesDto Supplies { get; set; } = new SuppliesDto();
    public EquipmentDto Equipment { get; set; } = new EquipmentDto();
    public OtherDirectCosts OtherDirectCosts { get; set; } = new OtherDirectCosts();
    public ConsultantDto Consultant { get; set; } = new ConsultantDto();
}

public record Resume
{
    public string FileName { get; set; } = string.Empty;
    public string ResumeMediaId { get; set; } = string.Empty;
}

public abstract record Subtier
{
    public string Description { get; set; } = string.Empty;
    public string DescriptionMediaId { get; set; }
}

public record TravelDto : Subtier
{
    public int NumberOfTrips { get; set; } = 0;
    public int NumberOfTravelers { get; set; } = 0;
    public string LocationOfGovEndUser { get; set; } = string.Empty;
    public string LocationOfSubcontractor { get; set; } = string.Empty;
    public bool UseRideshare { get; set; } = false;
    public bool UseRentalCar { get; set; } = false;
}

public record SubcontractorDto : Subtier
{
    public string Url { get; set; } = string.Empty;
}

public record ConsultantDto : Subtier
{
    public string Url { get; set; } = string.Empty;
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
