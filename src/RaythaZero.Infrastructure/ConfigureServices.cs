using RaythaZero.Infrastructure.Persistence.Interceptors;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Infrastructure.Persistence;
using RaythaZero.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using RaythaZero.Infrastructure.FileStorage;
using RaythaZero.Application.Common.Utils;
using RaythaZero.Infrastructure.BackgroundTasks;
using Microsoft.Extensions.Hosting;
using RaythaZero.Infrastructure.Configurations;
using Npgsql;
using RaythaZero.Infrastructure.GenerativeAi;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var dbConnectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddDbContext<RaythaDbContext>(options =>
        {
            options.UseNpgsql(dbConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5);
            });
        });
        services.AddTransient<IDbConnection>(_ => new NpgsqlConnection(dbConnectionString));
        services.AddScoped<IRaythaDbContext>(provider => provider.GetRequiredService<RaythaDbContext>());


        services.AddSingleton<ICurrentOrganizationConfiguration, CurrentOrganizationConfiguration>();
        services.AddScoped<IEmailerConfiguration, EmailerConfiguration>();
        services.AddScoped<IEmailer, Emailer>();
        services.AddTransient<IBackgroundTaskDb, BackgroundTaskDb>();
        services.AddTransient<IRaythaRawDbInfo, RaythaRawDbInfo>();

        //generative ai
        services.Configure<OpenAiSettings>(configuration);
        services.AddScoped<IGenerativeAiService, OpenAiService>();
        
        //file storage provider
        var fileStorageProvider = configuration[FileStorageUtility.CONFIG_NAME].IfNullOrEmpty(FileStorageUtility.LOCAL).ToLower();
        if (fileStorageProvider == FileStorageUtility.LOCAL)
        {
            services.AddScoped<IFileStorageProvider, LocalFileStorageProvider>();
        }
        else if (fileStorageProvider.ToLower() == FileStorageUtility.AZUREBLOB)
        {
            services.AddScoped<IFileStorageProvider, AzureBlobFileStorageProvider>();
        }
        else if (fileStorageProvider.ToLower() == FileStorageUtility.S3)
        {
            services.AddScoped<IFileStorageProvider, S3FileStorageProvider>();
        }
        else
        {
            throw new NotImplementedException($"Unsupported file storage provider: {fileStorageProvider}");
        }

        for (int i = 0; i < Convert.ToInt32(configuration["NUM_BACKGROUND_WORKERS"] ?? "4"); i++)
        {
            services.AddSingleton<IHostedService, QueuedHostedService>();
        }
        services.AddScoped<IBackgroundTaskQueue, BackgroundTaskQueue>();

        return services;
    }
}