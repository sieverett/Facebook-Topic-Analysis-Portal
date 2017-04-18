﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Facebook;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;

namespace FacebookCivicInsights
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Boilerplate: read the configuration files for the current environment.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            // Boilerplate: add service and create Policy with options
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddMvc();
            services.AddSingleton(Configuration);

            // Register our repositories with ASP.NET Core to allow them to be injected
            // into our controllers. This preserves the same state between the controllers.
            string elasticSearchUrl = Configuration["elasticsearch:url"];
            string elasticSearchDefaultIndex = Configuration["elasticsearch:defaultIndex"];
            var postRepository = new ElasticSearchRepository<ScrapedPost>(elasticSearchUrl, elasticSearchDefaultIndex + "-post");
            services.AddSingleton<IDataRepository<ScrapedPost>>(postRepository);

            var pageRepository = new ElasticSearchRepository<ScrapedPage>(elasticSearchUrl, elasticSearchDefaultIndex + "-page");
            services.AddSingleton<IDataRepository<ScrapedPage>>(pageRepository);

            string facebookGraphAPIVersion = Configuration["facebook:graphAPIVersion"];
            string facebookAppId = Configuration["facebook:appId"];
            string facebookAppSecret = Configuration["facebook:appSecret"];
            var graphClient = new GraphClient(facebookGraphAPIVersion, facebookAppId, facebookAppSecret);
            services.AddSingleton(graphClient);

            var postScrapeRepository = new ElasticSearchRepository<PostScrapeEvent>(elasticSearchUrl, elasticSearchDefaultIndex + "-post-event");
            services.AddSingleton<IDataRepository<PostScrapeEvent>>(postScrapeRepository);

            var pageScrapeRepository = new ElasticSearchRepository<PageScrapeEvent>(elasticSearchUrl, elasticSearchDefaultIndex + "-page-event");
            services.AddSingleton<IDataRepository<PageScrapeEvent>>(pageScrapeRepository);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Boilerplate: add a Cross Origin Request policy and various helpers.
            app.UseCors("CorsPolicy");
            app.UseStaticFiles();
            app.UseDeveloperExceptionPage();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
