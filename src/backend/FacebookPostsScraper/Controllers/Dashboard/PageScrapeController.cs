using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Facebook;
using Facebook.Models;
using Facebook.Requests;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using FacebookPostsScraper.Data;
using Microsoft.AspNetCore.Mvc;
using FacebookPostsScraper.Data.Scraper;

namespace FacebookCivicInsights.Controllers.Dashboard
{
    [Route("/api/dashboard/page")]
    public class PageScrapeController : Controller
    {
        private PageScraper PageScraper { get; }
        private ElasticSearchRepository<PageScrapeEvent> PageScrapeRepository { get; }

        public PageScrapeController(PageScraper pageScraper, ElasticSearchRepository<PageScrapeEvent> pageScrapeRepository)
        {
            PageScraper = pageScraper;
            PageScrapeRepository = pageScrapeRepository;
        }

        [HttpPost("new/multiple")]
        public IEnumerable<ScrapedPage> AddMany([FromBody]IEnumerable<ScrapedPage> pages)
        {
            if (pages == null)
            {
                throw new InvalidOperationException("No pages");
            }

            foreach (ScrapedPage page in pages)
            {
                yield return Add(page);
            }
        }

        [HttpPost("new")]
        public ScrapedPage Add([FromBody]ScrapedPage page)
        {
            DateTime now = DateTime.Now;

            // Get the Facebook page and save the page.s
            Page facebookPage = PageScraper.VerifyFacebookPage(page.FacebookId);
            page.Id = Guid.NewGuid().ToString();
            page.FacebookId = facebookPage.Id;
            page.FirstScrape = now;
            page.LatestScrape = now;

            page.FanCountHistory.Add(new DatedFanCount
            {
                Date = now,
                FanCount = facebookPage.FanCount
            });

            page.Created = now;
            PageScraper.Save(page);

            return page;
        }

        [HttpGet("{id}")]
        public ScrapedPage Get(string id) => PageScraper.Get(id);

        [HttpPatch("{id}")]
        public ScrapedPage Update(string id, [FromBody]ScrapedPage updatedPage)
        {
            if (updatedPage == null)
            {
                throw new InvalidOperationException("No updated page");
            }

            ScrapedPage page = Get(id);
            if (page == null)
            {
                throw new InvalidOperationException("No such page");
            }

            page.Name = updatedPage.Name;
            page.FacebookId = PageScraper.VerifyFacebookPage(page.FacebookId).Id;

            return PageScraper.Save(page);
        }

        [HttpGet("all")]
        public PagedResponse AllPages(int pageNumber, int pageSize, OrderingType? order, DateTime? since, DateTime? until)
        {
            return PageScraper.All(pageNumber, pageSize, p => p.Created, order, p => p.Created, since, until);
        }

        [HttpDelete("{id}")]
        public ScrapedPage Delete(string id) => PageScraper.Delete(id);

        [HttpGet("scrape/{id}")]
        public PageScrapeEvent GetScrape(string id) => PageScrapeRepository.Get(id);

        [HttpGet("scrape/all")]
        public PagedResponse AllScrapes(int pageNumber, int pageSize, OrderingType? order, DateTime? since, DateTime? until)
        {
            return PageScrapeRepository.All(pageNumber, pageSize, p => p.ImportStart, order, p => p.ImportStart, since, until);
        }

        [HttpGet("scrape/export")]
        public IActionResult ExportPost(OrderingType? order, DateTime? since, DateTime? until)
        {
            byte[] serialized = PageScrapeRepository.Export(p => p.ImportStart, order, p => p.ImportStart, since, until, CsvSerialization.MapPageScrape);
            return File(serialized, "text/csv", "export.csv");
        }

        [HttpPost("scrape/scrape")]
        public PageScrapeEvent ScrapePages([FromBody]IEnumerable<string> request)
        {
            ScrapedPage[] pages = PageScraper.Scrape(request).ToArray();
            var pageScrapeEvent = new PageScrapeEvent
            {
                Id = Guid.NewGuid().ToString(),
                ImportStart = pages.FirstOrDefault()?.LatestScrape ?? DateTime.Now,
                ImportEnd = DateTime.Now,
                Pages = pages
            };

            return PageScrapeRepository.Save(pageScrapeEvent);
        }
    }
}
