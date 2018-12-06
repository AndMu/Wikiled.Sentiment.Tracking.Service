using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Sentiment.Tracking.Service.Logic;

namespace Wikiled.Sentiment.Tracking.Service.Sample.Controllers
{
    [Route("api/[controller]")]
    public class MonitorController : BaseMonitorController
    {
        public MonitorController(ILoggerFactory loggerFactory, ITrackingManager tracking)
            : base(loggerFactory, tracking)
        {
        }

        protected override void Prepare(SentimentRequest request)
        {
        }
    }
}
