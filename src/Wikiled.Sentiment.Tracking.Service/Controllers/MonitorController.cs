using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wikiled.Sentiment.Tracking.Api.Request;
using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Sentiment.Tracking.Service.Logic;
using Wikiled.Server.Core.ActionFilters;
using Wikiled.Server.Core.Controllers;

namespace Wikiled.Sentiment.Tracking.Service.Controllers
{
    [TypeFilter(typeof(RequestValidationAttribute))]
    [TypeFilter(typeof(RequestEnrichmentAttribute))]
    [Route("api/[controller]")]
    public class MonitorController : BaseController
    {
        private readonly ITrackingManager tracking;

        public MonitorController(ILoggerFactory loggerFactory, ITrackingManager tracking)
            : base(loggerFactory)
        {
            this.tracking = tracking ?? throw new ArgumentNullException(nameof(tracking));
        }

        [Route("sentiment")]
        [HttpPost]
        public IActionResult GetResult([FromBody] SentimentRequest request)
        {
            Dictionary<string, TrackingResult[]> results = new Dictionary<string, TrackingResult[]>();
            for (int i = 0; i < request.Keywords.Length; i++)
            {
                results[request.Keywords[i]] = GetSingle(request.Keywords[i], request.Type, request.Hours);
            }

            return Ok(results);
        }

        [Route("history")]
        [HttpPost]
        public IActionResult GetResultHistory([FromBody] SentimentRequest request)
        {
            var hours = request.Hours.Max();
            Dictionary<string, RatingRecord[]> results = new Dictionary<string, RatingRecord[]>();
            for (int i = 0; i < request.Keywords.Length; i++)
            {
                results[request.Keywords[i]] = GetSingleHistory(request.Keywords[i], request.Type, hours);
            }

            return Ok(results);
        }

        private RatingRecord[] GetSingleHistory(string keyword, string type, int hours)
        {
            var tracker = tracking.Resolve(keyword, type);
            return tracker.GetRatings(hours).OrderByDescending(item => item.Date).ToArray();
        }

        private TrackingResult[] GetSingle(string keyword, string type, int[] selectedSteps)
        {
            var tracker = tracking.Resolve(keyword, type);
            TrackingResult[] results = new TrackingResult[selectedSteps.Length];
            for (int i = 0; i < selectedSteps.Length; i++)
            {
                var step = selectedSteps[i];
                var result = new TrackingResult
                {
                    Average = tracker.CalculateAverageRating(step),
                    TotalMessages = tracker.Count(lastHours: step),
                    Hours = step
                };

                results[i] = result;
            }

            return results.ToArray();
        }
    }
}
