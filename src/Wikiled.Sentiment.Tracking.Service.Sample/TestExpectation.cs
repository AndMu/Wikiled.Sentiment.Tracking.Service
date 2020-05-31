using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Wikiled.Sentiment.Tracking.Api.Request;
using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.Sentiment.Tracking.Service.Sample
{
    public class TestExpectation : IHostedService
    {
        public TestExpectation(ITrackingManager manager)
        {
            manager.Resolve("AMD", Constant.DefaultType).AddRating(new RatingRecord{Id = "1", Date = DateTime.Now, Rating = 1});
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
