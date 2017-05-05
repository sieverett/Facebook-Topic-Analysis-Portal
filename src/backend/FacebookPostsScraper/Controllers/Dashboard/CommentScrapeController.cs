using System;
using System.Collections.Generic;
using System.Text;
using Facebook;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using FacebookCivicInsights.Data.Scraper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FacebookCivicInsights.Controllers.Dashboard
{
    [Route("/api/dashboard/scrape/comment")]
    public class CommentScrapeController : Controller
    {
        private CommentScraper CommentScraper { get; }
        private PostScraper PostScraper { get; }
        
        public CommentScrapeController(CommentScraper commentScraper, PostScraper postScraper)
        {
            CommentScraper = commentScraper;
            PostScraper = postScraper;
        }

        [HttpGet("{id}")]
        public ScrapedComment GetComment(string id) => CommentScraper.Get(id);

        [HttpPost("all")]
        public PagedResponse<ScrapedComment> AllComments([FromBody]ElasticSearchRequest request)
        {
            return CommentScraper.Paged(request.PageNumber, request.PageSize, request.Query, request.Sort);
        }

        [HttpPost("export/csv")]
        public IActionResult ExportAsCSV([FromBody]ElasticSearchRequest request)
        {
            IEnumerable<ScrapedComment> history = CommentScraper.All(request.Query, request.Sort).Data;

            byte[] serialized = CsvSerialization.Serialize(history, CsvSerialization.MapComment);
            return File(serialized, "text/csv", "export.csv");
        }

        [HttpPost("export/json")]
        public IActionResult ExportAsJson([FromBody]ElasticSearchRequest request)
        {
            IEnumerable<ScrapedComment> history = CommentScraper.All(request.Query, request.Sort).Data;

            byte[] serialized = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(history));
            return File(serialized, "application/json-download", "export.json");
        }

        public class CommentScrapeRequest
        {
            public string PostId { get; set; }
        }

        [HttpPost("scrape")]
        public IEnumerable<ScrapedComment> ScrapeComments([FromBody]CommentScrapeRequest request)
        {
            return CommentScraper.Scrape(PostScraper.Get(request?.PostId));
        }
    }
}
