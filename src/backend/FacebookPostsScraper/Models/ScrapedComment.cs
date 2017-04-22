using System;
using Facebook.Models;

namespace FacebookCivicInsights.Models
{
    public class ScrapedComment : Comment
    {
        public Post Post { get; set; }

        public DateTime Created { get; set; }
        public DateTime LastScraped { get; set; }
    }
}
