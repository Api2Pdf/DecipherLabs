using Microsoft.AspNetCore.Mvc;

namespace RaythaZero.Web.Areas.Public.Pages.Login;

public class LoginWithJwt : BasePublicLoginPageModel
{
    public async Task<IActionResult> OnGet(string developerName, string token, string returnUrl = null)
    {
        var command = new RaythaZero.Application.Login.Commands.LoginWithJwt.Command
        {
            DeveloperName = developerName,
            Token = token
        };
        var response = await Mediator.Send(command);
        if (response.Success)
        {
            await LoginWithClaims(response.Result, true);
            if (HasLocalRedirect(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToPage("/Projects/Index");
        }
        else
        {
            throw new UnauthorizedAccessException(response.Error);
        }
    }
}