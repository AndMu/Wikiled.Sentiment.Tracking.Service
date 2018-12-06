using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Wikiled.Sentiment.Tracking.Service.Logic
{
    public class SentimentRequest
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public string[] Keywords { get; set; }


        public int[] Hours { get; set; }
    }
}
