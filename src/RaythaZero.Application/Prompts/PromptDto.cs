using System.Linq.Expressions;
using RaythaZero.Application.AuthenticationSchemes;
using RaythaZero.Application.Common.Models;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Prompts;

public record PromptDto : BaseAuditableEntityDto
{
    public string Label { get; init; } = string.Empty;
    public string DeveloperName { get; init; } = string.Empty;
    public string PromptText { get; init; } = string.Empty;
    public string ResultType { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;

    public static Expression<Func<Prompt, PromptDto>> GetProjection()
    {
        return prompt => GetProjection(prompt);
    }
    public static PromptDto GetProjection(Prompt entity)
    {
        return new PromptDto 
        {
            Id = entity.Id,
            Label = entity.Label,
            DeveloperName = entity.DeveloperName,
            CreatorUserId = entity.CreatorUserId,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime,
            LastModifierUserId = entity.LastModifierUserId,
            IsActive = entity.IsActive,
            ResultType = entity.ResultType,
            PromptText = entity.PromptText
        };
    }
}