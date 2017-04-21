using System;
using Facebook.Models;

namespace FacebookCivicInsights.Models
{
    public class ScrapedComment : Comment
    {
        public string ParentId { get; set; }

        public DateTime Created { get; set; }
        public DateTime LastScraped { get; set; }
    }
}
