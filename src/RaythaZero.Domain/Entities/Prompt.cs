namespace RaythaZero.Domain.Entities;

public class ResultTypeConstants
{
    public const string SINGLE_VALUE = "SingleValue";
    public const string JSON_VALUE = "JsonValue";
}

public class Prompt : BaseAuditableEntity
{
    public string Label { get; set; } = string.Empty;
    public string DeveloperName { get; set; } = string.Empty;
    public string PromptText { get; set; } = string.Empty;
    public string ResultType { get; set; } = ResultTypeConstants.SINGLE_VALUE;
    public bool IsActive { get; set; } = true;
}
    