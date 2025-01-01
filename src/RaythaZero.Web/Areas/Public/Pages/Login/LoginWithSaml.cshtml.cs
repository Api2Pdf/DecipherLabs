using Microsoft.AspNetCore.Mvc;

namespace RaythaZero.Web.Areas.Public.Pages.Login;

public class LoginWithSaml : BasePublicLoginPageModel
{
    public async Task<IActionResult> OnPost(string developerName, string returnUrl = null)
    {
        var samlResponse = Request.Form["SAMLResponse"].ToString().Replace(" ", "+"); //can't figure out why spaces come in on the post
        if (Request.Form.ContainsKey("RelayState") && Request.Form["RelayState"].Count == 1)
            returnUrl = Request.Form["RelayState"].ToArray()[0].ToString();

        var command = new RaythaZero.Application.Login.Commands.LoginWithSaml.Command 
        { 
            DeveloperName = developerName,
            SAMLResponse = samlResponse
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