using System;
using Wikiled.Sentiment.Tracking.Api.Request;
using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Sentiment.Tracking.Service.Logic;

namespace Wikiled.Sentiment.Tracking.Service.Sample
{
    public class TestExpectation
    {
        public TestExpectation(ITrackingManager manager)
        {
            manager.Resolve("AMD", Constant.DefaultType).AddRating(new RatingRecord("1", DateTime.Now, 1));
        }
    }
}
