using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RaythaZero.Application.Common.Behaviors;
using System.Reflection;
using RaythaZero.Application.Projects.Commands;

namespace RaythaZero.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
        });

        services.AddScoped<BeginToGeneratePackage.BackgroundTask>();
        return services;
    }
}

