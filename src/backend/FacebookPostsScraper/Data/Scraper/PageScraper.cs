using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Facebook;
using Facebook.Models;
using Facebook.Requests;
using FacebookCivicInsights.Models;
using Nest;

namespace FacebookCivicInsights.Data.Scraper
{
    public class PageScraper : ElasticSearchRepository<ScrapedPage>
    {
        private GraphClient GraphClient { get; }

        public PageScraper(ConnectionSettings settings, string defaultIndex, GraphClient graphClient) : base(settings, defaultIndex)
        {
            GraphClient = graphClient;
        }

        public ScrapedPage Scrape(string pageId, bool save, DateTime start)
        {
            // Query the Facebook Graph API to get the page likes.
            Page facebookPage = GraphClient.GetPage<Page>(new PageRequest(pageId));

            var scrapedPage = new ScrapedPage
            {
                FacebookId = facebookPage.Id,
                Name = facebookPage.Name,
                Category = facebookPage.Category,
                FanCount = facebookPage.FanCount
            };
            scrapedPage.Date = start;

            return save ? Save(scrapedPage, Refresh.False) : scrapedPage;
        }

        public IEnumerable<ScrapedPage> Scrape(IEnumerable<string> pageIds, DateTime start)
        {
            foreach (string page in pageIds)
            {
                yield return Scrape(page, true, start);
            }
        }

        public ScrapedPage Closest(string name, DateTime date)
        {
            // Get all the pages with the display name within +- 1 week of the specified date.
            IEnumerable<ScrapedPage> pages = Paged(search: q =>
            {
                return q.Match(m => m.Field(p => p.Name).Query(name)) && q.DateRange(d =>
                {
                    return d.Field(p => p.Date).LessThanOrEquals(date.AddDays(3)).GreaterThanOrEquals(date.AddDays(-3));
                });
            }).Data.Where(p => p.Name == name);

            // Get the closest date to the specified date.
            IEnumerable<ScrapedPage> closestPages = pages.OrderBy(p => (p.Date - date).Duration());
            return closestPages.FirstOrDefault();
        }
    }
}
