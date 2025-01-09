using System.Linq.Expressions;
using CSharpVitamins;
using RaythaZero.Application.Common.Models;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Projects;

public record ProjectDto : BaseAuditableEntityDto
{
    public string Label { get; init; }
    public dynamic ProjectData { get; init; }
    public bool? IsArchived { get; init; }
    
    public static Expression<Func<Project, ProjectDto>> GetProjection()
    {
        return entity => GetProjection(entity);
    }

    public static ProjectDto GetProjection(Project entity)
    {
        return new ProjectDto 
        {
            Id = entity.Id,
            Label = entity.Label,
            IsArchived = entity.IsArchived,
            ProjectData = entity.ProjectData,
        };
    }
}

public record ProjectLevelInfoDto
{
    public string DsipProposalNumber { get; init; } = string.Empty;
    public string TypeOfProposal { get; init; } = string.Empty;
    public ShortGuid? TopLevel { get; init; }
    public ShortGuid? ServiceSpecific { get; init; }
    public ShortGuid? Topic { get; init; }
    public IEnumerable<string> OtherDirectCostSelections { get; init; } = new List<string>();
    public IEnumerable<ResumeDto> Resumes { get; init; } = new List<ResumeDto>();
    public TravelDto Travel { get; init; } = new TravelDto();
    public MaterialsDto Materials { get; init; } = new MaterialsDto();
    public SuppliesDto Supplies { get; init; } = new SuppliesDto();
    public EquipmentDto Equipment { get; init; } = new EquipmentDto();
    public OtherDirectCosts OtherDirectCosts { get; init; } = new OtherDirectCosts();
    public ConsultantDto Consultant { get; init; } = new ConsultantDto();
}

public record ResumeDto
{
    public string FileName { get; init; } = string.Empty;
    public ShortGuid? ResumeMediaId { get; init; } = string.Empty;
}

public abstract record SubtierDto
{
    public string Description { get; init; } = string.Empty;
    public ShortGuid? DescriptionMediaId { get; init; }
}

public record TravelDto : SubtierDto
{
    public int NumberOfTrips { get; init; } = 0;
    public int NumberOfTravelers { get; init; } = 0;
    public string LocationOfGovEndUser { get; init; } = string.Empty;
    public string LocationOfSubcontractor { get; init; } = string.Empty;
    public bool UseRideshare { get; init; } = false;
    public bool UseRentalCar { get; init; } = false;
}

public record SubcontractorDto : SubtierDto
{
    public string Url { get; init; } = string.Empty;
}

public record ConsultantDto : SubtierDto
{
    public string Url { get; init; } = string.Empty;
}

public record SuppliesDto : SubtierDto
{
}

public record EquipmentDto : SubtierDto
{
}

public record OtherDirectCosts : SubtierDto
{
}

public record MaterialsDto : SubtierDto
{
}