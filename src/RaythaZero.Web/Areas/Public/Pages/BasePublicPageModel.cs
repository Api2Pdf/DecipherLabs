using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Web.Areas.Shared.Models;

namespace RaythaZero.Web.Areas.Public.Pages;

[Area("Public")]
[Authorize]
public class BasePublicPageModel : BasePageModel
{
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
}