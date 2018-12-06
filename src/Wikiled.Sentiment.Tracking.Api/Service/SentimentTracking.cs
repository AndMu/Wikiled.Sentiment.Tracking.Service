using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Common.Net.Client;
using Wikiled.Sentiment.Tracking.Api.Request;
using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.Sentiment.Tracking.Api.Service
{
    public class SentimentTracking : ISentimentTracking
    {
        private readonly IApiClient client;

        public SentimentTracking(IApiClientFactory factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            client = factory.GetClient();
            client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<IDictionary<string, TrackingResults>> GetTrackingResults(SentimentRequest request, CancellationToken token)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var result = await client.PostRequest<SentimentRequest, RawResponse<Dictionary<string, TrackingResults>>>("sentiment", request, token).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                throw new ApplicationException("Failed to retrieve:" + result.HttpResponseMessage);
            }

            return result.Result.Value;
        }

        public async Task<IDictionary<string, RatingRecord[]>> GetTrackingHistory(SentimentRequest request, CancellationToken token)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var result = await client
                .PostRequest<SentimentRequest, RawResponse<Dictionary<string, RatingRecord[]>>>("history", request, token)
                .ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                throw new ApplicationException("Failed to retrieve:" + result.HttpResponseMessage);
            }

            return result.Result.Value;
        }
    }
}
