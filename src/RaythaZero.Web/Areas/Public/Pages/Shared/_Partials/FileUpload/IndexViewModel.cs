namespace RaythaZero.Web.Areas.Public.Pages.Shared._Partials;

public record IndexViewModel
{
    public int CurrentNumberOfFiles { get; set; } = 0;
    public int MaxFiles { get; set; }
    public string SuccessEndpoint { get; set; }
    public bool HideUploaderIfMaxHit { get; set; } = true;
    public IEnumerable<string> AllowedFileTypes { get; set; } = new List<string> { "*/*" };
    public string AllowedFileTypesAsJson => System.Text.Json.JsonSerializer.Serialize(AllowedFileTypes);
}