using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Facebook;
using Facebook.Models;
using Facebook.Requests;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using FacebookPostsScraper.Data.Scraper;

namespace FacebookCivicInsights.Controllers.Dashboard
{
    [Route("/api/dashboard/comment")]
    public class CommentScrapeController : Controller
    {
        private CommentScraper CommentScraper { get; }

        public CommentScrapeController(CommentScraper commentScraper)
        {
            CommentScraper = commentScraper;
        }

        [HttpGet("{id}")]
        public Comment GetComment(string id) => CommentScraper.Get(id);

        [HttpGet("all")]
        public PagedResponse AllComments(string pageId, int pageNumber, int pageSize, OrderingType? order)
        {
            Func<QueryContainerDescriptor<ScrapedComment>, QueryContainer> search = q => q.Term(t => t.Field(c => c.ParentId).Value(pageId));
            return CommentScraper.All(pageNumber, pageSize, p => p.CreatedTime, order, search);
        }

        public class CommentScrapeRequest
        {
            public string PostId { get; set; }
        }

        [HttpPost("scrape")]
        public IEnumerable<ScrapedComment> ScrapeComments([FromBody]CommentScrapeRequest request)
        {
            return CommentScraper.Scrape(request?.PostId);
        }
    }
}
