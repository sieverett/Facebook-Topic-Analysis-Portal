using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Facebook;
using Facebook.Models;
using Facebook.Requests;
using FacebookCivicInsights.Models;
using Nest;
using System.Linq.Expressions;

namespace FacebookCivicInsights.Data.Scraper
{
    public class PageScraper : ElasticSearchRepository<ScrapedPage>
    {
        private GraphClient GraphClient { get; }

        public PageScraper(ConnectionSettings settings, string defaultIndex, GraphClient graphClient) : base(settings, defaultIndex)
        {
            GraphClient = graphClient;
        }

        public ScrapedPage Scrape(PageMetadata page, bool save, DateTime start)
        {
            // Query the Facebook Graph API to get the page likes.
            Page facebookPage = GraphClient.GetPage<Page>(new PageRequest(page.FacebookId));

            var scrapedPage = new ScrapedPage
            {
                FacebookId = facebookPage.Id,
                Name = page.Name,
                Category = facebookPage.Category,
                FanCount = facebookPage.FanCount
            };
            scrapedPage.Date = start;

            return save ? Save(scrapedPage, Refresh.False) : scrapedPage;
        }

        public IEnumerable<ScrapedPage> Scrape(PageMetadata[] pages, DateTime start)
        {
            Console.WriteLine($"Started scraping {pages.Length} pages");

            for (int i = 0; i < pages.Length;i++)
            {
                PageMetadata page = pages[i];
                Console.WriteLine($"{i + 1}/{pages.Length}: {page.Name}");

                yield return Scrape(page, true, start);
            }

            Console.WriteLine($"Done scraping {pages.Length} pages");
        }

        public ScrapedPage Closest(Expression<Func<ScrapedPage, string>> field, string query, DateTime date)
        {
            // Get all the pages with the display name within +- 1 week of the specified date.
            IEnumerable<ScrapedPage> pages = Paged(search: q =>
            {
                return q.Match(m => m.Field(field).Query(query)) && q.DateRange(d =>
                {
                    return d.Field("date").LessThanOrEquals(date.AddDays(4)).GreaterThanOrEquals(date.AddDays(-4));
                });
            }).Data.Where(p => field.Compile()(p) == query);

            // Get the closest date to the specified date.
            IEnumerable<ScrapedPage> closestPages = pages.OrderBy(p => (p.Date - date).Duration());
            return closestPages.FirstOrDefault();
        }
    }
}
