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
    public class PageScraper : ElasticSearchRepository<ScrapedPage>
    {
        private GraphClient GraphClient { get; }

        public PageScraper(string url, string defaultIndex, GraphClient graphClient) : base(url, defaultIndex)
        {
            GraphClient = graphClient;
        }

        public Page VerifyFacebookPage(string facebookId)
        {
            // Query the Facebook Graph API to make sur the page with the ID exists.
            Page facebookPage = GraphClient.GetPage<Page>(new PageRequest(facebookId));
            if (facebookPage == null)
            {
                throw new InvalidOperationException($"No such page {facebookId}");
            }

            if (Paged(search: q => q.Term("facebookId", facebookId)).Data.Any())
            {
                throw new InvalidOperationException($"Page already exists {facebookId}");
            }

            return facebookPage;
        }

        public IEnumerable<ScrapedPage> Scrape(IEnumerable<string> pageIds)
        {
            // If no specific pages were specified, scrape them all.
            IEnumerable<ScrapedPage> pages;
            if (pageIds == null)
            {
                pages = Paged().AllData();
            }
            else
            {
                pages = pageIds.Select(id => Get(id));
            }

            DateTime start = DateTime.Now;

            foreach (ScrapedPage page in pages)
            {
                Debug.Assert(page != null);

                // Query the Facebook Graph API to get the page likes.
                Page facebookPage = GraphClient.GetPage<Page>(new PageRequest(page.FacebookId));
                Debug.Assert(facebookPage != null);

                // Update the database with the new information.
                page.FirstScrape = page.FirstScrape ?? start;
                page.LatestScrape = start;
                page.FanCountHistory.Insert(0, new DatedFanCount
                {
                    Date = start,
                    FanCount = facebookPage.FanCount
                });

                Save(page, Refresh.False);

                // Don't store the entire fan count history for each page on each scrape.
                page.FanCountHistory = new List<DatedFanCount> { page.FanCountHistory.First() };

                yield return page;
            }
        }
    }
}
