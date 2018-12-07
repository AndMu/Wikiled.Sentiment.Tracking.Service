using System;
using Microsoft.Extensions.Logging;
using Wikiled.Sentiment.Tracking.Api.Request;

namespace Wikiled.Sentiment.Tracking.Service.Logic
{
    public class RequestEnrichment : IRequestEnrichment
    {
        private readonly ILogger<RequestEnrichment> logger;

        public RequestEnrichment(ILogger<RequestEnrichment> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public SentimentRequest Enrich(SentimentRequest request)
        {
            if (request.Type == null)
            {
                logger.LogDebug("Type not specified, switching to default {0}", Constant.DefaultType);
                request.Type = Constant.DefaultType;
            }

            if (request.Hours == null)
            {
                logger.LogDebug("Hours not specified, switching to default");
                request.Hours = new[] {24, 12, 6, 1};
            }

            if (request.Hour == default)
            {
                logger.LogDebug("Hour not specified, switching to default - 24");
                request.Hour = 24;
            }

            return request;
        }
    }
}
