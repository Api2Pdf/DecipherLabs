using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using RaythaZero.Application;
using RaythaZero.Application.Common.Utils;
using RaythaZero.Infrastructure.Persistence;
using RaythaZero.Web.Areas.Admin.Pages.Login;
using RaythaZero.Web.Middlewares;

namespace RaythaZero.Web;

public class Startup
{ 
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
        });
        services.AddApplicationServices();
        services.AddInfrastructureServices(Configuration);
        services.AddWebUIServices();
        services.AddRazorPages();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        string pathBase = Configuration["PATHBASE"] ?? string.Empty;
        app.UsePathBase(new PathString(pathBase));
        app.UseForwardedHeaders();
        app.UseExceptionHandler(ExceptionsMiddleware.ErrorHandler(pathBase));

        if (!env.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseStaticFiles();

        var fileStorageProvider = Configuration[FileStorageUtility.CONFIG_NAME].IfNullOrEmpty(FileStorageUtility.LOCAL).ToLower();
        var localStorageDirectory = Configuration[FileStorageUtility.LOCAL_DIRECTORY_CONFIG_NAME].IfNullOrEmpty(FileStorageUtility.DEFAULT_LOCAL_DIRECTORY);
        if (fileStorageProvider == FileStorageUtility.LOCAL)
        {
            var fullPath = Path.Combine(env.ContentRootPath, localStorageDirectory);
            Directory.CreateDirectory(fullPath);        
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    fullPath),
                    RequestPath = new PathString("/_static-files")
            });
        }

        app.UseSwagger(c =>
        {
            c.RouteTemplate = "admin/api/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"{pathBase}/admin/api/v1/swagger.json", "Raytha API - V1");
            c.RoutePrefix = $"admin/api";
        });

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
        });

        app.UseCors();
        
        bool applyMigrationsOnStartup = Convert.ToBoolean(Configuration["APPLY_PENDING_MIGRATIONS"] ?? "false");
        if (applyMigrationsOnStartup)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<RaythaDbContext>().Database.Migrate();
            }
        }
    } 
}