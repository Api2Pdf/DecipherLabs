using System.Linq.Expressions;
using CSharpVitamins;
using RaythaZero.Application.Common.Models;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Projects;

public record ProjectDto : BaseAuditableEntityDto
{
    public string Label { get; init; }
    public ProjectLevelInfo ProjectData { get; init; }
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