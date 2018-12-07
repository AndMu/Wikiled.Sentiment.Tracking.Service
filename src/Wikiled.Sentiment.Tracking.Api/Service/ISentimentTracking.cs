using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Sentiment.Tracking.Api.Request;
using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.Sentiment.Tracking.Api.Service
{
    public interface ISentimentTracking
    {
        Task<IDictionary<string, TrackingResult[]>> GetTrackingResults(SentimentRequest request, CancellationToken token);

        Task<IDictionary<string, RatingRecord[]>> GetTrackingHistory(SentimentRequest request, CancellationToken token);
    }
}
