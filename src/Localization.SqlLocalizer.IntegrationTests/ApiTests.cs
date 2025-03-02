﻿using Localization.SqlLocalizer.DbStringLocalizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Localization.SqlLocalizer.IntegrationTests
{
    public class ApiTests
    {
        [Test]
        public async Task GetExistingItem()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    var sqlConnectionString = "Data Source=.\\LocalizationRecords.sqlite";

                    services.AddDbContext<LocalizationModelContext>(options =>
                        options.UseSq(
                            sqlConnectionString,
                            b => b.MigrationsAssembly("AspNetCoreLocalization")
                        )
                    );

                    var useTypeFullNames = false;
                    var useOnlyPropertyNames = false;
                    var returnOnlyKeyIfNotFound = false;
                    var createNewRecordWhenLocalisedStringDoesNotExist = false;

                    services.AddSqlLocalization(options => options.UseSettings(
                        useTypeFullNames,
                        useOnlyPropertyNames,
                        returnOnlyKeyIfNotFound,
                        createNewRecordWhenLocalisedStringDoesNotExist));

                    services.AddMvc()
                      .AddViewLocalization()
                      .AddDataAnnotationsLocalization();

                    services.Configure<RequestLocalizationOptions>(
                        options =>
                        {
                            var supportedCultures = new List<CultureInfo>
                                {
                            new CultureInfo("en-US"),
                            new CultureInfo("de-CH"),
                            new CultureInfo("fr-CH"),
                            new CultureInfo("it-CH")
                                };

                            options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                            options.SupportedCultures = supportedCultures;
                            options.SupportedUICultures = supportedCultures;
                        });

                })
                .Configure(app =>
                {

                    var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
                    app.UseRequestLocalization(locOptions.Value);

                    app.UseStaticFiles();

                    app.UseMvc();

                });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                var response = await client.GetStringAsync("api/about?culture=de-CH");
                Assert.AreEqual("About in German", response);
            }
        }

        [Test]
        public async Task GetNonExistingItemAllOptionsFalse()
        {
            //var sqlSqlContextOptions = new SqlContextOptions();
            //var options = new DbContextOptions<LocalizationModelContext>();

            //var context = new LocalizationModelContext(options, "");


            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<LocalizationModelContext>(opt => opt.UseInMemoryDatabase("test"));

                    var useTypeFullNames = false;
                    var useOnlyPropertyNames = false;
                    var returnOnlyKeyIfNotFound = false;
                    var createNewRecordWhenLocalisedStringDoesNotExist = false;

                    services.AddSqlLocalization(options => options.UseSettings(
                        useTypeFullNames,
                        useOnlyPropertyNames,
                        returnOnlyKeyIfNotFound,
                        createNewRecordWhenLocalisedStringDoesNotExist));

                    services.AddMvc()
                      .AddViewLocalization()
                      .AddDataAnnotationsLocalization();

                    services.Configure<RequestLocalizationOptions>(
                        options =>
                        {
                            var supportedCultures = new List<CultureInfo>
                                {
                            new CultureInfo("en-US"),
                            new CultureInfo("de-CH"),
                            new CultureInfo("fr-CH"),
                            new CultureInfo("it-CH")
                                };

                            options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                            options.SupportedCultures = supportedCultures;
                            options.SupportedUICultures = supportedCultures;
                        });

                })
                .Configure(app =>
                {

                    var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
                    app.UseRequestLocalization(locOptions.Value);

                    app.UseStaticFiles();

                    app.UseMvc();

                });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                var response = await client.GetStringAsync("api/about/non?culture=de-CH");
                Assert.AreEqual("AboutController.AboutTitleNon.de-CH", response);
            }
        }

        [Test]
        public async Task GetNonExistingItemUsePropertiesOnly()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<LocalizationModelContext>(opt => opt.UseInMemoryDatabase("test"));

                    var useTypeFullNames = false;
                    var useOnlyPropertyNames = true;
                    var returnOnlyKeyIfNotFound = false;
                    var createNewRecordWhenLocalisedStringDoesNotExist = false;

                    services.AddSqlLocalization(options => options.UseSettings(
                        useTypeFullNames,
                        useOnlyPropertyNames,
                        returnOnlyKeyIfNotFound,
                        createNewRecordWhenLocalisedStringDoesNotExist));

                    services.AddMvc()
                      .AddViewLocalization()
                      .AddDataAnnotationsLocalization();

                    services.Configure<RequestLocalizationOptions>(
                        options =>
                        {
                            var supportedCultures = new List<CultureInfo>
                                {
                            new CultureInfo("en-US"),
                            new CultureInfo("de-CH"),
                            new CultureInfo("fr-CH"),
                            new CultureInfo("it-CH")
                                };

                            options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                            options.SupportedCultures = supportedCultures;
                            options.SupportedUICultures = supportedCultures;
                        });

                })
                .Configure(app =>
                {

                    var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
                    app.UseRequestLocalization(locOptions.Value);

                    app.UseStaticFiles();

                    app.UseMvc();
                });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                var response = await client.GetStringAsync("api/about/non?culture=de-CH");
                Assert.AreEqual("global.AboutTitleNon.de-CH", response);
            }
        }

        [Test]
        public async Task GetNonExistingItemUseTypeFullNames()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<LocalizationModelContext>(opt => opt.UseInMemoryDatabase("test"));

                    var useTypeFullNames = true;
                    var useOnlyPropertyNames = false;
                    var returnOnlyKeyIfNotFound = false;
                    var createNewRecordWhenLocalisedStringDoesNotExist = false;

                    services.AddSqlLocalization(options => options.UseSettings(
                        useTypeFullNames,
                        useOnlyPropertyNames,
                        returnOnlyKeyIfNotFound,
                        createNewRecordWhenLocalisedStringDoesNotExist));

                    services.AddMvc()
                      .AddViewLocalization()
                      .AddDataAnnotationsLocalization();

                    services.Configure<RequestLocalizationOptions>(
                        options =>
                        {
                            var supportedCultures = new List<CultureInfo>
                                {
                            new CultureInfo("en-US"),
                            new CultureInfo("de-CH"),
                            new CultureInfo("fr-CH"),
                            new CultureInfo("it-CH")
                                };

                            options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                            options.SupportedCultures = supportedCultures;
                            options.SupportedUICultures = supportedCultures;
                        });

                })
                .Configure(app =>
                {

                    var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
                    app.UseRequestLocalization(locOptions.Value);

                    app.UseStaticFiles();
                    app.UseMvc();
                });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                var response = await client.GetStringAsync("api/about/non?culture=de-CH");
                Assert.AreEqual("Localization.SqlLocalizer.IntegrationTests.Controllers.AboutController.AboutTitleNon.de-CH", response);
            }
        }

        //[Test]
        public async Task GetNonExistingItemUseTypeFullNamesReturnOnlyKey()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<LocalizationModelContext>(opt => opt.UseInMemoryDatabase("test"));

                    var useTypeFullNames = false;
                    var useOnlyPropertyNames = false;
                    var returnOnlyKeyIfNotFound = true;
                    var createNewRecordWhenLocalisedStringDoesNotExist = false;

                    services.AddSqlLocalization(options => options.UseSettings(
                        useTypeFullNames,
                        useOnlyPropertyNames,
                        returnOnlyKeyIfNotFound,
                        createNewRecordWhenLocalisedStringDoesNotExist));

                    services.AddMvc()
                      .AddViewLocalization()
                      .AddDataAnnotationsLocalization();

                    services.Configure<RequestLocalizationOptions>(
                        options =>
                        {
                            var supportedCultures = new List<CultureInfo>
                                {
                            new CultureInfo("en-US"),
                            new CultureInfo("de-CH"),
                            new CultureInfo("fr-CH"),
                            new CultureInfo("it-CH")
                                };

                            options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                            options.SupportedCultures = supportedCultures;
                            options.SupportedUICultures = supportedCultures;
                        });

                })
                .Configure(app =>
                {

                    var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
                    app.UseRequestLocalization(locOptions.Value);

                    app.UseStaticFiles();

                    app.UseMvc();

                    //app.Run(context =>
                    //{
                    //    var response = String.Format("Hello, Universe! It is {0}", DateTime.Now);
                    //    return context.Response.WriteAsync(response);
                    //});
                });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                var response = await client.GetStringAsync("api/about/non?culture=fr-CH");
                Assert.AreEqual("AboutTitleNon", response);
            }
        }

        [Test]
        public async Task GetNonExistingItemAllOptionsAddNewItem()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<LocalizationModelContext>(opt => opt.UseInMemoryDatabase("test"), ServiceLifetime.Singleton);

                    var useTypeFullNames = false;
                    var useOnlyPropertyNames = false;
                    var returnOnlyKeyIfNotFound = false;
                    var createNewRecordWhenLocalisedStringDoesNotExist = true;


                    // Requires that LocalizationModelContext is defined
                    // _createNewRecordWhenLocalisedStringDoesNotExist read from the dev env. 
                    services.AddSqlLocalization(options => options.UseSettings(
                        useTypeFullNames,
                        useOnlyPropertyNames,
                        returnOnlyKeyIfNotFound,
                        createNewRecordWhenLocalisedStringDoesNotExist));

                    services.AddMvc()
                      .AddViewLocalization()
                      .AddDataAnnotationsLocalization();

                    services.Configure<RequestLocalizationOptions>(
                        options =>
                        {
                            var supportedCultures = new List<CultureInfo>
                                {
                            new CultureInfo("en-US"),
                            new CultureInfo("de-CH"),
                            new CultureInfo("fr-CH"),
                            new CultureInfo("it-CH")
                                };

                            options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                            options.SupportedCultures = supportedCultures;
                            options.SupportedUICultures = supportedCultures;
                        });

                })
                .Configure(app =>
                {

                    var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
                    app.UseRequestLocalization(locOptions.Value);

                    app.UseStaticFiles();

                    app.UseMvc();
                });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                var response1 = await client.GetStringAsync("api/about/non?culture=de-CH");
                Assert.AreEqual("AboutController.AboutTitleNon.de-CH", response1);

                var response = await client.GetStringAsync("api/about/non?culture=es-ES");
                Assert.AreEqual("AboutController.AboutTitleNon.en-US", response);

                //var responseCount = await client.GetStringAsync("api/about/noncount?culture=de-CH");
                //Assert.AreEqual("2", responseCount);
            }
        }

        //[Test]
        public async Task AddNewItemDeveopmentMode()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<LocalizationModelContext>(options =>
                        options.UseInMemoryDatabase(databaseName: "Add_writes_to_database")
                    );

                    var useTypeFullNames = false;
                    var useOnlyPropertyNames = false;
                    var returnOnlyKeyIfNotFound = false;
                    var createNewRecordWhenLocalisedStringDoesNotExist = true;

                    services.AddSqlLocalization(options => options.UseSettings(
                        useTypeFullNames,
                        useOnlyPropertyNames,
                        returnOnlyKeyIfNotFound,
                        createNewRecordWhenLocalisedStringDoesNotExist));

                    services.AddMvc()
                      .AddViewLocalization()
                      .AddDataAnnotationsLocalization();

                    services.Configure<RequestLocalizationOptions>(
                        options =>
                        {
                            var supportedCultures = new List<CultureInfo>
                                {
                            new CultureInfo("en-US"),
                            new CultureInfo("de-CH"),
                            new CultureInfo("fr-CH"),
                            new CultureInfo("it-CH")
                                };

                            options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                            options.SupportedCultures = supportedCultures;
                            options.SupportedUICultures = supportedCultures;
                        });

                })
                .Configure(app =>
                {

                    var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
                    app.UseRequestLocalization(locOptions.Value);

                    app.UseStaticFiles();

                    app.UseMvc();

                });

            using (var server = new TestServer(builder))
            {
                var client = server.CreateClient();

                var response = await client.GetStringAsync("api/about/devmodetest?culture=de-CH");
                Assert.AreEqual("devmodetest.de-CH", response);

                var responseAgain = await client.GetStringAsync("api/about/devmodetest?culture=de-CH");
                Assert.AreEqual("devmodetest.de-CH", responseAgain);

                var response2 = await client.GetStringAsync("api/about/devmodetest2?culture=it-CH");
                Assert.AreEqual("devmodetest2.it-CH", response2);
            }
        }

    }
}
