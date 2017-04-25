using System;
using System.Collections.Generic;

namespace FacebookCivicInsights.Models
{
    public class PostScrapeHistory
    {
        public string Id { get; set; }

        public DateTime ImportStart { get; set; }
        public DateTime ImportEnd { get; set; }

        public DateTime? Since { get; set; }
        public DateTime? Until { get; set; }
        public IEnumerable<PageMetadata> Pages { get; set; }
        public int NumberOfPosts { get; set; }
        public int NumberOfComments { get; set; }
    }
}
