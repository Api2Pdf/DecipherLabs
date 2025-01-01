using Microsoft.AspNetCore.Mvc;
using RaythaZero.Application.AuthenticationSchemes.Queries;
using RaythaZero.Domain.ValueObjects;

namespace RaythaZero.Web.Areas.Public.Pages.Login;

public class LoginWithSso : BasePublicLoginPageModel
{
    public async Task<IActionResult> OnGet(string developerName, string returnUrl = "")
    {
        try
        {
            var response = await Mediator.Send(new GetAuthenticationSchemeByName.Query
                { DeveloperName = developerName });

            if (!response.Result.IsEnabledForAdmins)
            {
                return Unauthorized();
            }

            if (response.Result.AuthenticationSchemeType.DeveloperName ==
                AuthenticationSchemeType.Jwt.DeveloperName)
            {
                return Redirect(RelativeUrlBuilder.GetSingleSignOnCallbackJwtUrl("Public",
                    response.Result.DeveloperName, response.Result.SignInUrl, returnUrl));
            }

            if (response.Result.AuthenticationSchemeType.DeveloperName ==
                AuthenticationSchemeType.Saml.DeveloperName)
            {
                return Redirect(RelativeUrlBuilder.GetSingleSignOnCallbackSamlUrl("Public",
                    response.Result.DeveloperName, response.Result.SamlIdpEntityId, response.Result.SignInUrl,
                    returnUrl));
            }

            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            return RedirectToPage("/Login/LoginWithEmailAndPassword");
        }
    }
}