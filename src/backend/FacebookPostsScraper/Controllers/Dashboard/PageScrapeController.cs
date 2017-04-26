using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Facebook;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using FacebookPostsScraper.Data;
using Microsoft.AspNetCore.Mvc;
using FacebookPostsScraper.Data.Scraper;
using FacebookPostsScraper.Data.Importer;
using Elasticsearch.Net;

namespace FacebookCivicInsights.Controllers.Dashboard
{
    [Route("/api/dashboard/scrape/page")]
    public class PageScrapeController : Controller
    {
        private PageScraper PageScraper { get; }
        private ElasticSearchRepository<PageMetadata> PageMetadataRepository { get; }
        private ElasticSearchRepository<PageScrapeHistory> PageScrapeHistoryRepository { get; }

        public PageScrapeController(PageScraper pageScraper, ElasticSearchRepository<PageMetadata> pageMetadataRepository, ElasticSearchRepository<PageScrapeHistory> pageScrapeRepository)
        {
            PageScraper = pageScraper;
            PageMetadataRepository = pageMetadataRepository;
            PageScrapeHistoryRepository = pageScrapeRepository;
        }

        [HttpGet("{id}")]
        public ScrapedPage GetScrape(string id) => PageScraper.Get(id);

        [HttpGet("all")]
        public PagedResponse AllScrapes(int pageNumber, int pageSize, OrderingType? order, DateTime? since, DateTime? until)
        {
            return PageScraper.All(pageNumber, pageSize, p => p.Date, order, p => p.Date, since, until);
        }

        [HttpPost("scrape")]
        public PageScrapeHistory ScrapePages([FromBody]IEnumerable<string> request)
        {
            // If no pages were specified, scrape them all.
            IEnumerable<PageMetadata> pagesToScrape;
            if (request == null)
            {
                pagesToScrape = PageMetadataRepository.Paged().AllData();
            }
            else
            {
                pagesToScrape = request.Select(id => PageMetadataRepository.Get(id));
            }

            DateTime scrapeStart = DateTime.Now;
            ScrapedPage[] pages = PageScraper.Scrape(pagesToScrape.Select(p => p.FacebookId), scrapeStart).ToArray();

            // Now update the per-page list of all scraped pages.
            foreach (PageMetadata pageMetadata in pagesToScrape)
            {
                ScrapedPage scrapedPage = pages.First(p => p.FacebookId == pageMetadata.FacebookId);
                pageMetadata.FanCountHistory.Insert(0, new DatedFanCount
                {
                    Date = scrapedPage.Date,
                    FanCount = scrapedPage.FanCount,
                });
                pageMetadata.LatestScrape = scrapeStart;
                PageMetadataRepository.Save(pageMetadata, Refresh.False);

                // Only save the fan count on this date.
                pageMetadata.FanCountHistory = pageMetadata.FanCountHistory.Take(1).ToList();
            }

            // Now update the total-page list of the scrape.
            var pageScrapeHistory = new PageScrapeHistory
            {
                Id = Guid.NewGuid().ToString(),
                ImportStart = scrapeStart,
                ImportEnd = DateTime.Now,
                Pages = pagesToScrape
            };

            return PageScrapeHistoryRepository.Save(pageScrapeHistory);
        }

        [HttpGet("import")]
        public IEnumerable<ScrapedPage> ImportPages()
        {
            var importer = new ScrapeImporter(PageScraper, null);
            IEnumerable<string> files = Directory.EnumerateFiles("C:\\Users\\hughb\\Documents\\TAF\\Data", "*.csv", SearchOption.AllDirectories);
            IEnumerable<string> fanCountFiles = files.Where(f => f.Contains("count"));

            return importer.ImportPages(fanCountFiles);
        }

        [HttpGet("history/{id}")]
        public PageScrapeHistory GetScrapeHistory(string id) => PageScrapeHistoryRepository.Get(id);

        [HttpGet("history/all")]
        public PagedResponse AllScrapeHistory(int pageNumber, int pageSize, OrderingType? order, DateTime? since, DateTime? until)
        {
            return PageScrapeHistoryRepository.All(pageNumber, pageSize, p => p.ImportStart, order, p => p.ImportStart, since, until);
        }

        [HttpGet("history/export")]
        public IActionResult ExportPages(OrderingType? order, DateTime? since, DateTime? until)
        {
            byte[] serialized = PageScrapeHistoryRepository.Export(p => p.ImportStart, order, p => p.ImportStart, since, until, CsvSerialization.MapPageScrape);
            return File(serialized, "text/csv", "export.csv");
        }
    }
}
