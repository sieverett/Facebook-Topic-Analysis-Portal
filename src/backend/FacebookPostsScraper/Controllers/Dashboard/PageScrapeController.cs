using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Elasticsearch.Net;
using Facebook;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using Microsoft.AspNetCore.Mvc;
using FacebookCivicInsights.Data.Scraper;
using FacebookCivicInsights.Data.Importer;
using Newtonsoft.Json;

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
        public PagedResponse AllScrapes(int pageNumber, int pageSize, bool? descending, DateTime? since, DateTime? until)
        {
            return PageScraper.All<TimeSearchResponse<ScrapedPage>, ScrapedPage>(
                new PagedResponse(pageNumber, pageSize),
                new Ordering<ScrapedPage>("date", descending),
                p => p.Date, since, until);
        }

        [HttpPost("scrape")]
        public PageScrapeHistory ScrapePages([FromBody]IEnumerable<string> request)
        {
            // If no pages were specified, scrape them all.
            PageMetadata[] pagesToScrape;
            if (request == null)
            {
                pagesToScrape = PageMetadataRepository.Paged().AllData().ToArray();
            }
            else
            {
                pagesToScrape = request.Select(id => PageMetadataRepository.Get(id)).ToArray();
            }

            DateTime scrapeStart = DateTime.Now;
            ScrapedPage[] pages = PageScraper.Scrape(pagesToScrape, scrapeStart).ToArray();

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
            var importer = new ScrapeImporter(PageScraper, PageMetadataRepository, null);
            IEnumerable<string> files = Directory.EnumerateFiles("C:\\Users\\hughb\\Documents\\TAF\\Data", "*.csv", SearchOption.AllDirectories);
            IEnumerable<string> fanCountFiles = files.Where(f => f.Contains("count"));

            return importer.ImportPages(fanCountFiles);
        }

        [HttpGet("history/{id}")]
        public PageScrapeHistory GetScrapeHistory(string id) => PageScrapeHistoryRepository.Get(id);

        [HttpGet("history/all")]
        public PagedResponse<PageScrapeHistory> AllScrapeHistory(int pageNumber, int pageSize, bool? descending, DateTime? since, DateTime? until)
        {
            return PageScrapeHistoryRepository.All<TimeSearchResponse<PageScrapeHistory>, PageScrapeHistory>(
                new PagedResponse(pageNumber, pageSize),
                new Ordering<PageScrapeHistory>("importStart", descending),
                p => p.ImportStart, since, until
            );
        }

        [HttpGet("history/export/csv")]
        public IActionResult ExportPagesAsCSV(bool? descending, DateTime? since, DateTime? until)
        {
            IEnumerable<PageScrapeHistory> history = AllScrapeHistory(0, int.MaxValue, descending, since, until).AllData();

            byte[] serialized = CsvSerialization.Serialize(history, CsvSerialization.MapPageScrape);
            return File(serialized, "text/csv", "export.csv");
        }

        [HttpGet("history/export/json")]
        public IActionResult ExportPagesAsJson(bool? descending, DateTime? since, DateTime? until)
        {
            IEnumerable<PageScrapeHistory> history = AllScrapeHistory(0, int.MaxValue, descending, since, until).AllData();

            byte[] serialized = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(history));
            return File(serialized, "application/json-download", "export.json");
        }
    }
}
