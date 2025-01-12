namespace RaythaZero.Web.Areas.Public.Pages.Shared._Partials;

public record IndexViewModel
{
    public int MaxFiles { get; set; }
    public string SuccessEndpoint { get; set; }
    
    public string ContainerId { get; set; }
}