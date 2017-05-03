using System;
using Facebook.Models;

namespace FacebookCivicInsights.Models
{
    public class ScrapedComment : Comment
    {
        public ScrapedPost Post { get; set; }

        public DateTime FirstScraped { get; set; }
        public DateTime LastScraped { get; set; }
    }
}
