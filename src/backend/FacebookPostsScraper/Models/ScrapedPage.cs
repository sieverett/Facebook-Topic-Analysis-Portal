using System;

namespace FacebookCivicInsights.Models
{
    public class ScrapedPage
    {
        public string Id { get; set; }
        public string FacebookId { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public int FanCount { get; set; }
        public DateTime Date { get; set; }
    }
}
