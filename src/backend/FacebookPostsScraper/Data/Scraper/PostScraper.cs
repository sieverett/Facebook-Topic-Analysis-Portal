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

            Console.WriteLine($"Started scraping {pages.Length} pages for their posts");

            DateTime start = DateTime.Now;
            int numberOfPosts = 0;
            for (int i = 0; i < pages.Length; i++)
            {
                PageMetadata page = pages[i];
                Console.WriteLine($"{i + 1}/{pages.Length}: {page.Name}");

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
                    UpdateMetadata(post, page.Name);
                    Save(post, Refresh.False);

                    numberOfPosts++;
                    yield return post;
                }

                Console.WriteLine(numberOfPosts);
            }

            Console.WriteLine($"Done scraping {pages.Length} pages for their posts");
        }

        public ScrapedPost ScrapePost(string postId) => GraphClient.GetPost<ScrapedPost>(new PostRequest(postId));
        
        public void UpdateMetadata(ScrapedPost post, string pageName)
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

            ScrapedPage scrapedPage = PageScraper.Closest(p => p.Name, pageName, post.CreatedTime);
            post.Page = scrapedPage;

            post.LastScraped = DateTime.Now;
            if (post.Scraped == DateTime.MinValue)
            {
                post.Scraped = post.LastScraped;
            }
        }
    }
}
