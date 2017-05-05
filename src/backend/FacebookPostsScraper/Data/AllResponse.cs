using System.Collections.Generic;

namespace FacebookCivicInsights.Data
{
    public class AllResponse<T> where T: class, new()
    {
        public long TotalCount { get; set; }
        public IEnumerable<T> Data { get; set; }
    }
}
