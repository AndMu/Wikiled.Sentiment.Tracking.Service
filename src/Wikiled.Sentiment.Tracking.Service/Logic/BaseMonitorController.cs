using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Server.Core.ActionFilters;
using Wikiled.Server.Core.Controllers;

namespace Wikiled.Sentiment.Tracking.Service.Logic
{
    [TypeFilter(typeof(RequestValidationAttribute))]
    public abstract class BaseMonitorController : BaseController
    {
        private readonly ITrackingManager tracking;

        protected BaseMonitorController(ILoggerFactory loggerFactory, ITrackingManager tracking)
            : base(loggerFactory)
        {
            this.tracking = tracking ?? throw new ArgumentNullException(nameof(tracking));
        }

        [Route("sentiment")]
        [HttpPost]
        public IActionResult GetResult([FromBody] SentimentRequest request)
        {
            Prepare(request);
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
            Prepare(request);
            var hours = request.Hours.FirstOrDefault();
            if (hours == default)
            {
                Logger.LogInformation("Hours not specified, switching to default");
                hours = 24;
            }

            Dictionary<string, RatingRecord[]> results = new Dictionary<string, RatingRecord[]>();
            for (int i = 0; i < request.Keywords.Length; i++)
            {
                results[request.Keywords[i]] = GetSingleHistory(request.Keywords[i], request.Type, hours);
            }

            return Ok(results);
        }

        protected abstract void Prepare(SentimentRequest request);

        private RatingRecord[] GetSingleHistory(string keyword, string type, int hours)
        {
            var tracker = tracking.Resolve(keyword, type);
            return tracker.GetRatings(hours).OrderByDescending(item => item.Date).ToArray();
        }

        private TrackingResults GetSingle(string keyword, string type, int[] selectedSteps)
        {
            var tracker = tracking.Resolve(keyword, type);
            TrackingResults result = new TrackingResults { Keyword = tracker.Name, Type = tracker.Type };

            int[] steps = { 24, 12, 6, 1 };
            if (selectedSteps != null)
            {
                steps = selectedSteps;
            }

            foreach (int step in steps)
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
