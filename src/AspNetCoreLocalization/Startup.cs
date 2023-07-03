using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Localization.SqlLocalizer.DbStringLocalizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace AspNetCoreLocalization;

public class Startup
{
    private readonly bool _createNewRecordWhenLocalisedStringDoesNotExist;

    public Startup(IHostingEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

        builder.AddEnvironmentVariables();
        Configuration = builder.Build();

        if (env.IsDevelopment()) _createNewRecordWhenLocalisedStringDoesNotExist = true;
    }

    public IConfigurationRoot Configuration { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        // services.AddLocalization(options => options.ResourcesPath = "Resources");

        var sqlConnectionString = Configuration["DbStringLocalizer:ConnectionString"];

        services.AddDbContext<LocalizationModelContext>(options =>
                options.UseSqlite(
                    sqlConnectionString,
                    b => b.MigrationsAssembly("AspNetCoreLocalization")
                ),
            ServiceLifetime.Singleton,
            ServiceLifetime.Singleton
        );

        var useTypeFullNames = false;
        var useOnlyPropertyNames = false;
        var returnOnlyKeyIfNotFound = false;

        // Requires that LocalizationModelContext is defined
        // _createNewRecordWhenLocalisedStringDoesNotExist read from the dev env. 
        services.AddSqlLocalization(options => options.UseSettings(
            useTypeFullNames,
            useOnlyPropertyNames,
            returnOnlyKeyIfNotFound,
            _createNewRecordWhenLocalisedStringDoesNotExist));
        // services.AddSqlLocalization(options => options.ReturnOnlyKeyIfNotFound = true);
        // services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.AddScoped<LanguageActionFilter>();

        services.Configure<RequestLocalizationOptions>(
            options =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new("en-US"),
                    new("de-CH"),
                    new("fr-CH"),
                    new("it-CH")
                };

                options.DefaultRequestCulture = new RequestCulture("en-US", "en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

        services.AddMvc().AddViewLocalization()
            .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                {
                    var assemblyName = new AssemblyName(typeof(SharedResource).GetTypeInfo().Assembly.FullName);
                    return factory.Create("SharedResource", assemblyName.Name);
                };
            });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "API"
            });
        });
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
        app.UseRequestLocalization(locOptions.Value);

        app.UseStaticFiles();

        app.UseMvc(routes =>
        {
            routes.MapRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}");
        });

        // Enable middleware to serve generated Swagger as a JSON endpoint.
        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1"); });
    }
}