using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RaythaZero.Application.Common.Behaviors;
using System.Reflection;
using RaythaZero.Application.Projects.Commands;
using RaythaZero.Application.Projects.Extractors;

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

        services.AddHttpClient<BeginToGeneratePackage.BackgroundTask>();
        services.AddScoped<BeginToGeneratePackage.BackgroundTask>();
        services.AddScoped<ResumeExtractor>();
        services.AddScoped<PersonGenerator>();
        services.AddScoped<BenefitsExtractor>();
        services.AddScoped<FringeGenerator>();
        services.AddScoped<TravelGenerator>();
        services.AddScoped<TravelCostExtractor>();
        return services;
    }
}

