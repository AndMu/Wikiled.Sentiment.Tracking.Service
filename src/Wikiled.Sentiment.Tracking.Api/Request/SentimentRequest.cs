using System.ComponentModel.DataAnnotations;

namespace Wikiled.Sentiment.Tracking.Api.Request
{
    public class SentimentRequest
    {
        public string Type { get; set; }

        [Required]
        public string[] Keywords { get; set; }

        public int[] Hours { get; set; }
    }
}
