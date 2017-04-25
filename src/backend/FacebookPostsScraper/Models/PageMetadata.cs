using System;
using System.Collections.Generic;

namespace FacebookCivicInsights.Models
{
    public class PageMetadata
    {
        public string Id { get; set; }

        public string FacebookId { get; set; }
        public string Name { get; set; }

        public DateTime Created { get; set; }

        public DateTime? FirstScrape { get; set; }
        public DateTime? LatestScrape { get; set; }

        public int? FanCount
        {
            get
            {
                if (FanCountHistory.Count == 0)
                {
                    return null;
                }

                return FanCountHistory[FanCountHistory.Count - 1].FanCount;
            }
        }

        private List<DatedFanCount> _fanCountHistory;
        public List<DatedFanCount> FanCountHistory
        {
            get => _fanCountHistory ?? (_fanCountHistory = new List<DatedFanCount>());
            set => _fanCountHistory = value;
        }
    }

    public class DatedFanCount
    {
        public DateTime Date { get; set; }
        public int FanCount { get; set; }
    }
}
