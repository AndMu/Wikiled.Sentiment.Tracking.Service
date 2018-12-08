using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Wikiled.Sentiment.Tracking.Service.Sample
{
    public class Startup : BaseStartup
    {
        public Startup(ILoggerFactory loggerFactory, IHostingEnvironment env)
            : base(loggerFactory, env)
        {
        }

        protected override void ConfigureSpecific(ContainerBuilder builder)
        {
            builder.RegisterType<TestExpectation>().AsSelf().AutoActivate();
        }

        protected override string GetPersistencyLocation()
        {
            return "Data";
        }
    }
}
