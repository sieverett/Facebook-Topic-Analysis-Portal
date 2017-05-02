using System;
using System.Collections.Generic;
using System.Text;
using Facebook;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using FacebookCivicInsights.Data.Scraper;
using Microsoft.AspNetCore.Mvc;
using Nest;
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

        [HttpGet("post/{postId}")]
        public PagedResponse AllCommentsForPage(string postId, int pageNumber, int pageSize, bool? descending)
        {
            Func<QueryContainerDescriptor<ScrapedComment>, QueryContainer> search = q => q.Term(t => t.Field(c => c.Post.Id).Value(postId));
            return CommentScraper.Paged<TimeSearchResponse<ScrapedComment>>(
                new PagedResponse(pageNumber, pageSize),
                new Ordering<ScrapedComment>("created_time", descending),
                search);
        }

        [HttpGet("all")]
        public PagedResponse<ScrapedComment> AllComments(int pageNumber, int pageSize, string orderingKey, bool? descending, DateTime? since, DateTime? until)
        {
            return CommentScraper.All<TimeSearchResponse<ScrapedComment>, ScrapedComment>(
                new PagedResponse(pageNumber, pageSize),
                new Ordering<ScrapedComment>(orderingKey ?? "created_time", descending),
                p => p.CreatedTime, since, until);
        }

        [HttpGet("export/csv")]
        public IActionResult ExportAsCSV(bool? descending, DateTime? since, DateTime? until)
        {
            IEnumerable<ScrapedComment> history = AllComments(0, int.MaxValue, null, descending, since, until).AllData();

            byte[] serialized = CsvSerialization.Serialize(history, CsvSerialization.MapComment);
            return File(serialized, "text/csv", "export.csv");
        }

        [HttpGet("export/json")]
        public IActionResult ExportAsJson(bool? descending, DateTime? since, DateTime? until)
        {
            IEnumerable<ScrapedComment> history = AllComments(0, int.MaxValue, null, descending, since, until).AllData();

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
