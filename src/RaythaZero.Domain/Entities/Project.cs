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
    public IEnumerable<string> Resumes { get; set; } = new List<string>();
    public Travel Travel { get; set; } = new Travel();
    public Materials Materials { get; set; } = new Materials();
    public Supplies Supplies { get; set; } = new Supplies();
    public Equipment Equipment { get; set; } = new Equipment();
    public OtherDirectCosts OtherDirectCosts { get; set; } = new OtherDirectCosts();
    public Consultant Consultant { get; set; } = new Consultant();
    public Subcontractor Subcontractor { get; set; } = new Subcontractor();
}


public abstract record AbstractSubtier
{
    public string Description { get; set; } = string.Empty;
    public string DescriptionMediaId { get; set; } = string.Empty;
}

public record Travel : AbstractSubtier
{
    public int NumberOfTrips { get; set; } = 0;
    public int NumberOfTravelers { get; set; } = 0;
    public string LocationOfGovEndUser { get; set; } = string.Empty;
    public string LocationOfSubcontractor { get; set; } = string.Empty;
    public bool UseRideshare { get; set; } = false;
    public bool UseRentalCar { get; set; } = false;
}

public record Subcontractor : AbstractSubtier
{
    public string Url { get; set; } = string.Empty;
}

public record Consultant : AbstractSubtier
{
    public string Url { get; set; } = string.Empty;
}

public record Supplies : AbstractSubtier
{
}

public record Equipment : AbstractSubtier
{
}

public record OtherDirectCosts : AbstractSubtier
{
}

public record Materials : AbstractSubtier
{
}
