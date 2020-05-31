using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Net.Client;
using Wikiled.Common.Utilities.Modules;
using Wikiled.Sentiment.Api.Request;
using Wikiled.Sentiment.Api.Service;
using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Sentiment.Tracking.Modules;
using Wikiled.Sentiment.Tracking.Service.Config;
using Wikiled.Sentiment.Tracking.Service.Controllers;
using Wikiled.Sentiment.Tracking.Service.Logic;
using Wikiled.Server.Core.Errors;
using Wikiled.Server.Core.Helpers;
using Wikiled.Server.Core.Middleware;

namespace Wikiled.Sentiment.Tracking.Service
{
    public abstract class BaseStartup
    {
        private readonly ILogger<BaseStartup> logger;

        protected BaseStartup(ILoggerFactory loggerFactory, IWebHostEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Env = env;
            logger = loggerFactory.CreateLogger<BaseStartup>();
            Configuration.ChangeNlog();
            logger.LogInformation($"Starting: {Assembly.GetExecutingAssembly().GetName().Version}");
        }

        public IConfigurationRoot Configuration { get; }

        public IWebHostEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");
            app.UseRouting();
            app.UseExceptionHandlingMiddleware();
            app.UseHttpStatusCodeExceptionMiddleware();
            app.UseRequestLogging();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            applicationLifetime.ApplicationStopping.Register(OnShutdown);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            // Needed to add this section, and....
            services.AddCors(
                options =>
                {
                    options.AddPolicy(
                        "CorsPolicy",
                        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
                });

            services.AddControllers();
            services.AddOptions();

            SentimentConfig sentimentConfig = services.RegisterConfiguration<SentimentConfig>(Configuration.GetSection("sentiment"));

            // Create the container builder.
            services.RegisterModule<CommonModule>();
            services.AddTransient<ISentimentAnalysis, SentimentAnalysis>();
            services.AddTransient<IRequestEnrichment, RequestEnrichment>();

            ConfigureSpecific(services);
            SetupSentimentServices(services, sentimentConfig);
            SetupTracking(services);

            logger.LogInformation("Ready!");
        }

        protected virtual void OnShutdown()
        {
            logger.LogInformation("OnShutdown");
        }

        protected abstract void ConfigureSpecific(IServiceCollection builder);

        protected abstract string GetPersistencyLocation();

        private void SetupTracking(IServiceCollection builder)
        {
            var config = new TrackingConfiguration(TimeSpan.FromHours(1), TimeSpan.FromDays(10), Path.Combine(GetPersistencyLocation(), "ratings.csv"));
            logger.LogInformation("Setup tracking: {0}", config.Persistency);
            config.Restore = true;
            builder.RegisterModule(new TrackingModule(config));
        }

        private void SetupSentimentServices(IServiceCollection builder, SentimentConfig sentiment)
        {
            if (string.IsNullOrEmpty(sentiment.Url))
            {
                logger.LogInformation("Sentiment service is not configured.");
                return;
            }

            logger.LogInformation("Setting up sentiment services...");
            builder.AddSingleton< IStreamApiClientFactory>(context => new StreamApiClientFactory(context.GetRequiredService<ILoggerFactory>(),
                                                                   new HttpClient
                                                                   {
                                                                       Timeout = TimeSpan.FromMinutes(10)
                                                                   },
                                                                   new Uri(sentiment.Url)));
            var request = new WorkRequest
            {
                CleanText = true,
                Domain = sentiment.Domain
            };

            builder.AddSingleton(request);
            builder.AddTransient<ISentimentAnalysis, SentimentAnalysis>();
            logger.LogInformation("Register sentiment: {0} {1}", sentiment.Url, sentiment.Domain);
        }
    }
}
