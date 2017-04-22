using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Elasticsearch.Net;
using Facebook;
using Facebook.Models;
using Facebook.Requests;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;

namespace FacebookPostsScraper.Data.Scraper
{
    public class PostScraper : ElasticSearchRepository<ScrapedPost>
    {
        private GraphClient GraphClient { get; }

        public PostScraper(string url, string defaultIndex, GraphClient graphClient) : base(url, defaultIndex, i =>
        {
            // We need to tell Elasticsearch explicitly that this field is a geopoint.
            return i.Mappings(ms => ms.Map<ScrapedPost>(m => m.Properties(p =>
            {
                return p.GeoPoint(g => g.Name("geoPoint"));
            })));
        })
        {
            GraphClient = graphClient;
        }

        public IEnumerable<ScrapedPost> Scrape(IEnumerable<ScrapedPage> pages, DateTime since, DateTime until)
        {
            Debug.Assert(pages != null);
            Debug.Assert(since != DateTime.MinValue && since < DateTime.Now);
            Debug.Assert(until != DateTime.MinValue && since < DateTime.Now);
            Debug.Assert(since < until);

            DateTime start = DateTime.Now;
            int numberOfPosts = 0;

            foreach (ScrapedPage page in pages)
            {
                Console.WriteLine(page.Name);

                // Query the Facebook Graph API to get all posts in the given range, published only by
                // the page.
                var graphRequest = new PostsRequest(page.FacebookId, PostsRequestEdge.Posts)
                {
                    Since = since,
                    Until = until,
                    PaginationLimit = 100
                };

                PagedResponse<ScrapedPost> postsResponse = GraphClient.GetPosts<ScrapedPost>(graphRequest);
                foreach (ScrapedPost post in postsResponse.AllData())
                {
                    // Update the database with the new post.
                    Location location = post.Place?.Location;
                    if (location != null)
                    {
                        post.GeoPoint = $"{location.Latitude},{location.Longitude}";
                    }
                    else
                    {
                        post.GeoPoint = null;
                    }
                    post.Page = page;
                    post.Created = DateTime.Now;
                    post.LastScraped = start;
                    Save(post, Refresh.False);

                    numberOfPosts++;
                    yield return post;
                }

                // Don't store the entire fan count history for the page belonging to each post.
                page.FanCountHistory = page.FanCountHistory.Take(1).ToList();

                Console.WriteLine(numberOfPosts);
            }
        }
    }
}
