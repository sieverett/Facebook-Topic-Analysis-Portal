using System;
using System.Collections.Generic;
using Facebook.Models;

namespace FacebookCivicInsights.Models
{
    public class ScrapedPost : Post
    {
        public ScrapedPage Page { get; set; }

        public DateTime Scraped { get; set; }
        public DateTime LastScraped { get; set; }

        public string GeoPoint { get; set; }

        public IEnumerable<string> Topics { get; set; }
    }
}
