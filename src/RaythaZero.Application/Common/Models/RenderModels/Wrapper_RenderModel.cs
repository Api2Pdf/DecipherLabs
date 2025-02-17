﻿using RaythaZero.Application.Common.Interfaces;

namespace RaythaZero.Application.Common.Models.RenderModels;

public record Wrapper_RenderModel : IInsertTemplateVariable
{
    public CurrentOrganization_RenderModel CurrentOrganization { get; init; }
    public CurrentUser_RenderModel CurrentUser { get; init; }
    public object Target { get; init; }
    public Dictionary<string, string> QueryParams { get; init; } = new Dictionary<string, string>();

    public string? RequestVerificationToken { get; set; }
    public object? ViewData { get; set; }
    public string? PathBase { get; set; }

    public virtual IEnumerable<string> GetDeveloperNames()
    {
        yield return nameof(PathBase);
        yield return nameof(QueryParams);
        yield return nameof(RequestVerificationToken);
        yield return nameof(ViewData);
    }

    public virtual IEnumerable<KeyValuePair<string, string>> GetTemplateVariables()
    {
        foreach (var developerName in GetDeveloperNames())
        {
            yield return new KeyValuePair<string, string>(developerName, $"{developerName}");
        }
    }

    public virtual string GetTemplateVariablesAsForEachLiquidSyntax()
    {
        return string.Empty;
    }
}
