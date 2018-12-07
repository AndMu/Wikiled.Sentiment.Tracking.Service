using Wikiled.Sentiment.Tracking.Api.Request;

namespace Wikiled.Sentiment.Tracking.Service.Logic
{
    public interface IRequestEnrichment
    {
        SentimentRequest Enrich(SentimentRequest request);
    }
}