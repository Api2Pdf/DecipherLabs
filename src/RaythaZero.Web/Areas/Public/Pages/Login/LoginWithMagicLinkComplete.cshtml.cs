using Microsoft.AspNetCore.Mvc;

namespace RaythaZero.Web.Areas.Public.Pages.Login;

public class LoginWithMagicLinkComplete : BasePublicLoginPageModel
{
    public async Task<IActionResult> OnGet(string token = null, string returnUrl = null)
    {
        if (string.IsNullOrEmpty(token))
        {
            SetErrorMessage("Login token is missing.");
            return new ForbidResult();
        }
     
        var response = await Mediator.Send(new RaythaZero.Application.Login.Commands.CompleteLoginWithMagicLink.Command { Id = token });

        if (response.Success)
        {
            await LoginWithClaims(response.Result, true);
            if (HasLocalRedirect(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToPage("/Projects/Index");
            }
        }
        else
        {
            SetErrorMessage(response.Error);
            return RedirectToPage("/Login/LoginWithMagicLink", new { returnUrl });
        } 
    }
}