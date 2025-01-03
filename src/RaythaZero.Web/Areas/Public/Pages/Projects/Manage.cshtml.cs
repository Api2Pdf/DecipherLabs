using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Web.Areas.Public.Pages.Projects._Partials;

namespace RaythaZero.Web.Areas.Public.Pages.Projects;
public class Manage : BasePublicPageModel
{
    [BindProperty]
    public IFormFile ResumeFile { get; set; }

    public ResumeViewModel[] Resumes { get; set; }
        
    public void OnGet(string id)
    {
        ProjectId = "1";
        ProjectName = "This project name 1";
        Resumes = new ResumeViewModel[]
        {
            new ResumeViewModel { FileId = "1", FileName = "This file name 1", Src = "/htlld" },
            new ResumeViewModel { FileId = "2", FileName = "This file name 2", Src = "/htlld" },
            new ResumeViewModel { FileId = "3", FileName = "This file name 3", Src = "/htlld" },
        };
    }
    
    public async Task<IActionResult> OnGetResumes(string id)
    {
        ProjectId = "1";
        ProjectName = "This project name 1";
        Resumes = new ResumeViewModel[]
        {
            new() { FileId = "1", FileName = "This file name 1", Src = "/htlld" },
            new() { FileId = "2", FileName = "This file name 2", Src = "/htlld" },
            new() { FileId = "3", FileName = "This file name 3", Src = "/htlld" },
        };
        return new PartialViewResult { ViewName = "_Partials/Resumes", ViewData=ViewData };
    }

    public async Task<IActionResult> OnPostResumeUpload(string id)
    {
        if (ResumeFile != null && ResumeFile.Length > 0)
        {
            return new JsonResult(new { success = true });
        }
        else
        {
            return new JsonResult(new { success = false });
        }
    }

    public record ResumeViewModel
    {
        public string FileName { get; init; }
        public string FileId { get; init; }
        public string Src { get; init; }
    }
}