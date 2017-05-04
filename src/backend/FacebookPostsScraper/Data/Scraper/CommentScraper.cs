using System;
using System.Collections.Generic;
using System.Diagnostics;
using Elasticsearch.Net;
using Facebook;
using Facebook.Models;
using Facebook.Requests;
using FacebookCivicInsights.Models;
using Nest;

namespace FacebookCivicInsights.Data.Scraper
{
    public class CommentScraper : ElasticSearchRepository<ScrapedComment>
    {
        private GraphClient GraphClient { get; }

        public CommentScraper(ConnectionSettings settings, string defaultIndex, GraphClient graphClient) : base(settings, defaultIndex)
        {
            GraphClient = graphClient;
        }

        public IEnumerable<ScrapedComment> Scrape(ScrapedPost post)
        {
            Debug.Assert(post != null);
            var comments = new List<ScrapedComment>();
            DateTime now = DateTime.Now;

            CommentsRequest graphRequest = new CommentsRequest(post.Id) { PaginationLimit = 100 };
            PagedResponse<ScrapedComment> commentsResponse = GraphClient.GetComments<ScrapedComment>(graphRequest);

            // Could be null if the post doesn't exist anymore.
            if (commentsResponse != null)
            {
                foreach (ScrapedComment comment in commentsResponse.AllData())
                {
                    if (comment.FirstScraped == DateTime.MinValue)
                    {
                        comment.FirstScraped = now;
                    }
                    comment.LastScraped = now;
                    comment.Post = post;

                    comments.Add(Save(comment, Refresh.False));
                }
            }

            return comments;
        }
    }
}
