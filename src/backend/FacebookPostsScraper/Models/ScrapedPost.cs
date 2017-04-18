using System;
using Facebook.Models;

namespace FacebookCivicInsights.Models
{
    public class ScrapedPost : Post
    {
        public ScrapedPage Page { get; set; }

        public DateTime Created { get; set; }
        public DateTime LastScraped { get; set; }
    }
}
