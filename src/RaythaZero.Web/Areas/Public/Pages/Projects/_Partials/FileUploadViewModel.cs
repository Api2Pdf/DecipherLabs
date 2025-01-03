namespace RaythaZero.Web.Areas.Public.Pages.Projects._Partials;

public record FileUploadViewModel
{
    public string ContainerId { get; set; }
    public int MaxFiles { get; set; }
    public string Endpoint { get; set; }
    public string FileName { get; set; }
    public string TargetFrame { get; set; }
    public string TargetSrc { get; set; }
}