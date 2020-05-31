using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Wikiled.Sentiment.Tracking.Service.Sample
{
    public class Startup : BaseStartup
    {
        public Startup(ILoggerFactory loggerFactory, IWebHostEnvironment env)
            : base(loggerFactory, env)
        {
        }

        protected override void ConfigureSpecific(IServiceCollection builder)
        {
            builder.AddHostedService<TestExpectation>();
        }

        protected override string GetPersistencyLocation()
        {
            return "Data";
        }
    }
}
