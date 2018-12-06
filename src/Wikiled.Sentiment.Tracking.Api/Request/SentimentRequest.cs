using System;

namespace Wikiled.Sentiment.Tracking.Api.Request
{
    public class SentimentRequest
    {
        public SentimentRequest(params string[] keywords)
        {
            Keywords = keywords ?? throw new ArgumentNullException(nameof(keywords));
        }

        public string Type { get; set; }

        public string[] Keywords { get; }

        public int[] Hours { get; set; }
    }
}
