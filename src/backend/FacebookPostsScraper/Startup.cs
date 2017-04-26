using System;
using Facebook;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FacebookCivicInsights.Data.Scraper;
using Nest;

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

            Version facebookGraphAPIVersion = new Version(Configuration["facebook:graphAPIVersion"]);
            string facebookAppId = Configuration["facebook:appId"];
            string facebookAppSecret = Configuration["facebook:appSecret"];
            var graphClient = new GraphClient(facebookGraphAPIVersion, facebookAppId, facebookAppSecret);
            services.AddSingleton(graphClient);

            string elasticSearchUrl = Configuration["elasticsearch:url"];
            string elasticSearchDefaultIndex = Configuration["elasticsearch:defaultIndex"];

            string elasticSearchUserName = Configuration["elasticsearch:user"];
            string elasticSearchPassword = Configuration["elasticsearch:password"];

            var node = new Uri(elasticSearchUrl);
            Func<ConnectionSettings> settings = () => new ConnectionSettings(node).BasicAuthentication(elasticSearchUserName, elasticSearchPassword);

            var pageMetadataRepository = new ElasticSearchRepository<PageMetadata>(settings(), elasticSearchDefaultIndex + "-metadata-page");
            services.AddSingleton(pageMetadataRepository);

            var pageScrapeHistoryRepository = new ElasticSearchRepository<PageScrapeHistory>(settings(), elasticSearchDefaultIndex + "-metadata-pagescrape");
            services.AddSingleton(pageScrapeHistoryRepository);

            var postScrapeRepository = new ElasticSearchRepository<PostScrapeHistory>(settings(), elasticSearchDefaultIndex + "-metadata-postscrape");
            services.AddSingleton(postScrapeRepository);

            var pageScraper = new PageScraper(settings(), elasticSearchDefaultIndex + "-page", graphClient);
            services.AddSingleton(pageScraper);

            var postScraper = new PostScraper(settings(), elasticSearchDefaultIndex + "-post", pageScraper, graphClient);
            services.AddSingleton(postScraper);

            var commentScraper = new CommentScraper(settings(), elasticSearchDefaultIndex + "-comment", graphClient);
            services.AddSingleton(commentScraper);
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
