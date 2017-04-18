using System;
using System.Collections.Generic;

namespace FacebookCivicInsights.Models
{
    public class PageScrapeEvent
    {
        public string Id { get; set; }

        public DateTime ImportStart { get; set; }
        public DateTime ImportEnd { get; set; }

        public IEnumerable<ScrapedPage> Pages { get; set; }
    }
}
