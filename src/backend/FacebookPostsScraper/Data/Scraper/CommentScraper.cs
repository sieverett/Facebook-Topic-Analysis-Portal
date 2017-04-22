using Elasticsearch.Net;
using Facebook;
using Facebook.Requests;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FacebookPostsScraper.Data.Scraper
{
    public class CommentScraper : ElasticSearchRepository<ScrapedComment>
    {
        private GraphClient GraphClient { get; }

        public CommentScraper(string url, string defaultIndex, GraphClient graphClient) : base(url, defaultIndex)
        {
            GraphClient = graphClient;
        }

        public IEnumerable<ScrapedComment> Scrape(string parentId)
        {
            Debug.Assert(parentId != null);

            DateTime now = DateTime.Now;
            CommentsRequest graphRequest = new CommentsRequest(parentId) { PaginationLimit = 100 };
            foreach (ScrapedComment comment in GraphClient.GetComments<ScrapedComment>(graphRequest).AllData())
            {
                if (comment.Created == DateTime.MinValue)
                {
                    comment.Created = now;
                }
                comment.LastScraped = now;
                comment.ParentId = parentId;

                Save(comment, Refresh.False);

                yield return comment;
            }
        }
    }
}
