using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.Admins.Queries;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Web.Areas.Admin.Pages.Admins;

[Authorize(Policy = BuiltInSystemPermission.MANAGE_ADMINISTRATORS_PERMISSION)]
public class ResetPassword : BaseAdminPageModel, ISubActionViewModel
{
    [BindProperty]
    public FormModel Form { get; set; }

    public string Id { get; set; }
    public bool IsActive { get; set; }
    public async Task<IActionResult> OnGet(string id)
    {
        if (!CurrentOrganization.EmailAndPasswordIsEnabledForAdmins)
        {
            SetErrorMessage("Authentication scheme is disabled");
            return RedirectToPage("/Admins/Edit", new { id });
        }


        var response = await Mediator.Send(new GetAdminById.Query { Id = id });
        Form = new FormModel
        {
            Id = id
        };
        
        Id = id;
        IsActive = response.Result.IsActive;
        return Page();
    }

    public async Task<IActionResult> OnPost(string id)
    {
        var input = new RaythaZero.Application.Admins.Commands.ResetPassword.Command
        {
            Id = Form.Id,
            ConfirmNewPassword = Form.ConfirmNewPassword,
            NewPassword = Form.NewPassword,
            SendEmail = Form.SendEmail
        };
        var response = await Mediator.Send(input);

        if (response.Success)
        {
            SetSuccessMessage($"Password was reset successfully.");
            return RedirectToPage("/Admins/Index");
        }
        else
        {
            SetErrorMessage("There was an error attempting to reset this password. See the error below.", response.GetErrors());
            var admin = await Mediator.Send(new GetAdminById.Query { Id = id });
            Id = id;
            IsActive = admin.Result.IsActive;
            return Page();
        }
    }

    public record FormModel
    {
        public string Id { get; set; }

        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Display(Name = "Re-type the new password")]
        public string ConfirmNewPassword { get; set; }
        public bool SendEmail { get; set; } = true;
    }
}