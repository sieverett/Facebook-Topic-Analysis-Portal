using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Facebook;
using Facebook.Models;
using Facebook.Requests;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using Nest;

namespace FacebookPostsScraper.Data.Scraper
{
    public class PageScraper : ElasticSearchRepository<ScrapedPage>
    {
        private GraphClient GraphClient { get; }

        public PageScraper(ConnectionSettings settings, string defaultIndex, GraphClient graphClient) : base(settings, defaultIndex)
        {
            GraphClient = graphClient;
        }

        public ScrapedPage Scrape(string pageId, bool save)
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
            scrapedPage.Date = DateTime.Now;

            return save ? Save(scrapedPage, Refresh.False) : scrapedPage;
        }

        public IEnumerable<ScrapedPage> Scrape(IEnumerable<string> pageIds)
        {
            foreach (string page in pageIds)
            {
                yield return Scrape(page, save: true);
            }
        }

        public ScrapedPage Closest(string displayName, DateTime date)
        {
            // Get all the pages with the display name within +- 1 week of the specified date.
            IEnumerable<ScrapedPage> pages = Paged(search: q =>
            {
                return q.Match(m => m.Field("displayName").Query(displayName)) && q.DateRange(d =>
                {
                    return d.Field("date").LessThanOrEquals(date.AddDays(3)).GreaterThanOrEquals(date.AddDays(-3));
                });
            }).Data.Where(p => p.DisplayName == displayName);

            // Get the closest date to the specified date.
            IEnumerable<ScrapedPage> closestPages = pages.OrderBy(p => (p.Date - date).Duration());
            return closestPages.FirstOrDefault();
        }
    }
}
