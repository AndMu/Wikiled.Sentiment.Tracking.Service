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
            Dictionary<string, TrackingResults> results = new Dictionary<string, TrackingResults>();
            for (int i = 0; i < request.Keywords.Length; i++)
            {
                results[request.Keywords[i]] = GetSingle(request.Keywords[0], request.Type, request.Hours);
            }

            return Ok(results);
        }

        [Route("history")]
        [HttpPost]
        public IActionResult GetResultHistory([FromBody] SentimentRequest request)
        {
            var hours = request.Hours.FirstOrDefault();
            if (hours == default)
            {
                Logger.LogInformation("Hours not specified, switching to default");
                hours = 24;
            }

            Dictionary<string, RatingRecord[]> results = new Dictionary<string, RatingRecord[]>();
            for (int i = 0; i < request.Keywords.Length; i++)
            {
                results[request.Keywords[i]] = GetSingleHistory(request.Keywords[i], request.Type, request.Hour);
            }

            return Ok(results);
        }

        private RatingRecord[] GetSingleHistory(string keyword, string type, int hours)
        {
            var tracker = tracking.Resolve(keyword, type);
            return tracker.GetRatings(hours).OrderByDescending(item => item.Date).ToArray();
        }

        private TrackingResults GetSingle(string keyword, string type, int[] selectedSteps)
        {
            var tracker = tracking.Resolve(keyword, type);
            TrackingResults result = new TrackingResults { Keyword = tracker.Name, Type = tracker.Type };
            foreach (int step in selectedSteps)
            {
                result.Sentiment[$"{step}H"] = new TrackingResult
                {
                    Average = tracker.CalculateAverageRating(step),
                    TotalMessages = tracker.Count(lastHours: step)
                };
            }

            result.Total = tracker.Count(false);
            return result;
        }
    }
}
