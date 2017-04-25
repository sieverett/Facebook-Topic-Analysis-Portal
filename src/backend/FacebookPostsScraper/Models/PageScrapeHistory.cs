using System;
using System.Collections.Generic;

namespace FacebookCivicInsights.Models
{
    public class PageScrapeHistory
    {
        public string Id { get; set; }

        public DateTime ImportStart { get; set; }
        public DateTime ImportEnd { get; set; }

        public IEnumerable<ScrapedPage> Pages { get; set; }
    }
}
