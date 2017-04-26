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

namespace FacebookCivicInsights.Controllers.Dashboard
{
    [Route("/api/dashboard/scrape/page")]
    public class PageScrapeController : Controller
    {
        private PageScraper PageScraper { get; }
        private ElasticSearchRepository<PageScrapeHistory> PageScrapeHistoryRepository { get; }

        public PageScrapeController(PageScraper pageScraper, ElasticSearchRepository<PageScrapeHistory> pageScrapeRepository)
        {
            PageScraper = pageScraper;
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
            DateTime now = DateTime.Now;
            ScrapedPage[] pages = PageScraper.Scrape(request, now).ToArray();
            var pageScrapeHistory = new PageScrapeHistory
            {
                Id = Guid.NewGuid().ToString(),
                ImportStart = now,
                ImportEnd = DateTime.Now,
                Pages = pages
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
