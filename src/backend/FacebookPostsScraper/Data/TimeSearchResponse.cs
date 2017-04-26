using System;

namespace FacebookCivicInsights.Data
{
    public class TimeSearchResponse<T> : ElasticSearchPagedResponse<T> where T : class, new()
    {
        public DateTime? Since { get; set; }
        public DateTime? Until { get; set; }
    }
}
