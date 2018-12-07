using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Wikiled.Sentiment.Tracking.Api.Request;

namespace Wikiled.Sentiment.Tracking.Service.Logic
{
    public class RequestEnrichmentAttribute : ActionFilterAttribute
    {
        private readonly IRequestEnrichment enrichment;

        public RequestEnrichmentAttribute(IRequestEnrichment enrichment)
        {
            this.enrichment = enrichment ?? throw new ArgumentNullException(nameof(enrichment));
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var argument in context.ActionArguments.ToArray())
            {
                var request = argument.Value as SentimentRequest;
                context.ActionArguments[argument.Key] = enrichment.Enrich(request);
            }
        }
    }
}
