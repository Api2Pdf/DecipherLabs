using Microsoft.AspNetCore.Authorization;
using RaythaZero.Domain.Entities;
using MediatR;
using RaythaZero.Application.Login.Commands;
using RaythaZero.Application.Common.Exceptions;

namespace RaythaZero.Web.Authentication;

public interface IHasApiKeyRequirement : IAuthorizationRequirement
{
}

public class ApiIsAdminRequirement : IHasApiKeyRequirement
{
}

public class ApiManageUsersRequirement : IHasApiKeyRequirement
{
}

public class ApiManageSystemSettingsRequirement : IHasApiKeyRequirement
{
}

public class RaythaApiAuthorizationHandler : IAuthorizationHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor = null;
    private readonly IMediator _mediator;
    private const string X_API_KEY = "X-API-KEY";
    public const string POLICY_PREFIX = "API_";

    public RaythaApiAuthorizationHandler(IHttpContextAccessor httpContextAccessor, IMediator mediator)
    {
        _httpContextAccessor = httpContextAccessor;
        _mediator = mediator;
    }

    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();
        if (!pendingRequirements.Any(p => p is IHasApiKeyRequirement))
        {
            return;
        }
        if (!_httpContextAccessor.HttpContext.Request.Headers.Any(p => p.Key.ToUpper() == X_API_KEY))
        {
            throw new InvalidApiKeyException("Missing api key");
        }

        var apiKey = _httpContextAccessor.HttpContext.Request.Headers.FirstOrDefault(p => p.Key.ToUpper() == X_API_KEY).Value.FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidApiKeyException("Missing api key");
        }

        var user = await _mediator.Send(new LoginWithApiKey.Command { ApiKey = apiKey });

        if (!user.Success)
        {
            throw new InvalidApiKeyException(user.Error);
        }

        var systemPermissions = new List<string>();

        foreach (var role in user.Result.Roles)
        {
            systemPermissions.AddRange(role.SystemPermissions);
        }

        foreach (var requirement in pendingRequirements)
        {
            if (requirement is ApiIsAdminRequirement)
            {
                context.Succeed(requirement);
            }

            if (requirement is ApiManageSystemSettingsRequirement)
            {
                if (systemPermissions.Contains(BuiltInSystemPermission.MANAGE_SYSTEM_SETTINGS_PERMISSION))
                {
                    context.Succeed(requirement);
                }
            }
            else if (requirement is ApiManageUsersRequirement)
            {
                if (systemPermissions.Contains(BuiltInSystemPermission.MANAGE_USERS_PERMISSION))
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}