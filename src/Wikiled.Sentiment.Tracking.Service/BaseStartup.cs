using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        protected BaseStartup(ILoggerFactory loggerFactory, IHostingEnvironment env)
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

        public IHostingEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");
            app.UseExceptionHandlingMiddleware();
            app.UseHttpStatusCodeExceptionMiddleware();
            app.UseRequestLogging();
            app.UseMvc();
            applicationLifetime.ApplicationStopping.Register(OnShutdown);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Needed to add this section, and....
            services.AddCors(
                options =>
                {
                    options.AddPolicy(
                        "CorsPolicy",
                        itemBuider => itemBuider.AllowAnyOrigin()
                                                .AllowAnyMethod()
                                                .AllowAnyHeader()
                                                .AllowCredentials());
                });

            // Add framework services.
            services.AddMvc(options => { }).AddApplicationPart(typeof(MonitorController).Assembly).AddControllersAsServices();

            // needed to load configuration from appsettings.json
            services.AddOptions();

            SentimentConfig sentimentConfig = services.RegisterConfiguration<SentimentConfig>(Configuration.GetSection("sentiment"));

            // Create the container builder.
            var builder = new ContainerBuilder();
            builder.RegisterModule<CommonModule>();
            builder.RegisterType<SentimentAnalysis>().As<ISentimentAnalysis>();
            builder.RegisterType<RequestEnrichment>().As<IRequestEnrichment>();

            builder.Populate(services);
            ConfigureSpecific(builder);
            SetupSentimentServices(builder, sentimentConfig);
            SetupTracking(builder);

            IContainer appContainer = builder.Build();

            logger.LogInformation("Ready!");
            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(appContainer);
        }

        protected virtual void OnShutdown()
        {
            logger.LogInformation("OnShutdown");
        }

        protected abstract void ConfigureSpecific(ContainerBuilder builder);

        protected abstract string GetPersistencyLocation();

        private void SetupTracking(ContainerBuilder builder)
        {
            var config = new TrackingConfiguration(TimeSpan.FromHours(1), TimeSpan.FromDays(10), Path.Combine(GetPersistencyLocation(), "ratings.csv"));
            config.Restore = true;
            builder.RegisterModule(new TrackingModule(config));
        }

        private void SetupSentimentServices(ContainerBuilder builder, SentimentConfig sentiment)
        {
            if (string.IsNullOrEmpty(sentiment.Url))
            {
                logger.LogInformation("Sentiment service is not configured.");
                return;
            }

            logger.LogInformation("Setting up sentiment services...");
            builder.Register(context => new StreamApiClientFactory(context.Resolve<ILoggerFactory>(),
                                                                   new HttpClient
                                                                   {
                                                                       Timeout = TimeSpan.FromMinutes(10)
                                                                   },
                                                                   new Uri(sentiment.Url)))
                .As<IStreamApiClientFactory>();
            var request = new WorkRequest
            {
                CleanText = true,
                Domain = sentiment.Domain
            };

            builder.RegisterInstance(request);
            builder.RegisterType<SentimentAnalysis>().As<ISentimentAnalysis>();
            logger.LogInformation("Register sentiment: {0} {1}", sentiment.Url, sentiment.Domain);
        }
    }
}
