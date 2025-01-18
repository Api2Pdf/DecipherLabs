
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.BackgroundTasks.Queries;
using RaythaZero.Application.Projects.Commands;
using RaythaZero.Application.Projects.Queries;

namespace RaythaZero.Web.Areas.Public.Pages.Projects;

public class GeneratePackage : BasePublicPageModel
{
    public string BackgroundTaskId { get; set; } = string.Empty;
    public int PercentComplete { get; set; } = 0;
    public string StatusInfo { get; set; } = string.Empty; 
    public async Task<IActionResult> OnGet(string id, string backgroundTaskId = "")
    {
        var response = await Mediator.Send(new GetProjectById.Query { Id = id });
        ProjectName = response.Result.Label;
        ProjectId = id;

        if (!string.IsNullOrEmpty(backgroundTaskId))
        {
            var backgroundTask = await Mediator.Send(new GetBackgroundTaskById.Query { Id = backgroundTaskId });
            PercentComplete = backgroundTask.Result.PercentComplete;
            StatusInfo = backgroundTask.Result.StatusInfo;
            BackgroundTaskId = backgroundTaskId;
        }

        return Page();
    }

    public async Task<IActionResult> OnGetProgress(string id, string backgroundTaskId = "")
    {
        var response = await Mediator.Send(new GetProjectById.Query { Id = id });
        ProjectName = response.Result.Label;
        ProjectId = id;

        if (!string.IsNullOrEmpty(backgroundTaskId))
        {
            var backgroundTask = await Mediator.Send(new GetBackgroundTaskById.Query { Id = backgroundTaskId });
            PercentComplete = backgroundTask.Result.PercentComplete;
            StatusInfo = backgroundTask.Result.StatusInfo;
            BackgroundTaskId = backgroundTaskId;
        }

        return new PartialViewResult { ViewName = "_Partials/GeneratePackageProgress", ViewData=ViewData };
    }

    public async Task<IActionResult> OnPost(string id)
    {
        var response = await Mediator.Send(new BeginToGeneratePackage.Command { Id = id });
        if (response.Success)
        {
            SetSuccessMessage("Package is generating...");
            BackgroundTaskId = response.Result;
            return RedirectToPage("/Projects/GeneratePackage", new { id = id, backgroundTaskId = response.Result });
        }
        else
        {
            SetErrorMessage(response.GetErrors());
            return Page();
        }
    }
}