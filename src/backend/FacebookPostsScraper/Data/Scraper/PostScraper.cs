using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Elasticsearch.Net;
using Facebook;
using Facebook.Models;
using Facebook.Requests;
using FacebookCivicInsights.Models;
using Nest;

namespace FacebookCivicInsights.Data.Scraper
{
    public class PostScraper : ElasticSearchRepository<ScrapedPost>
    {
        private PageScraper PageScraper { get; }
        private GraphClient GraphClient { get; }

        public PostScraper(ConnectionSettings settings, string defaultIndex, PageScraper pageScraper, GraphClient graphClient) : base(settings, defaultIndex, i =>
        {
            // We need to tell Elasticsearch explicitly that this field is a geopoint.
            return i.Mappings(ms => ms.Map<ScrapedPost>(m => m.Properties(p =>
            {
                return p.GeoPoint(g => g.Name("geoPoint"));
            })));
        })
        {
            PageScraper = pageScraper;
            GraphClient = graphClient;
        }

        public IEnumerable<ScrapedPost> Scrape(PageMetadata[] pages, DateTime since, DateTime until)
        {
            Debug.Assert(pages != null);
            Debug.Assert(since != DateTime.MinValue && since < DateTime.Now);
            Debug.Assert(until != DateTime.MinValue && since < DateTime.Now);
            Debug.Assert(since < until);

            DateTime start = DateTime.Now;
            int numberOfPosts = 0;

            for (int i = 0; i < pages.Length; i++)
            {
                PageMetadata page = pages[i];
                Console.WriteLine($"{i}/{pages.Length}: page.Name");

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
                    UpdateMetadata(post);
                    Save(post, Refresh.False);

                    numberOfPosts++;
                    yield return post;
                }

                // Don't store the entire fan count history for the page belonging to each post.
                page.FanCountHistory = page.FanCountHistory.Take(1).ToList();

                Console.WriteLine(numberOfPosts);
            }
        }

        public ScrapedPost ScrapePost(string postId) => GraphClient.GetPost<ScrapedPost>(new PostRequest(postId));
        
        public void UpdateMetadata(ScrapedPost post)
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

            ScrapedPage scrapedPage = PageScraper.Closest(post.Page.Name, post.CreatedTime);
            post.Page = scrapedPage;

            if (post.Scraped != DateTime.MinValue)
            {
                post.Scraped = DateTime.Now;
            }
            post.LastScraped = DateTime.Now;
        }
    }
}
