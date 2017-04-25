using System;
using System.Collections.Generic;
using Facebook;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using FacebookPostsScraper.Data.Scraper;
using Microsoft.AspNetCore.Mvc;
using Nest;

namespace FacebookPostsScraper.Controllers.Dashboard
{
    [Route("/api/dashboard/scrape/comment")]
    public class CommentScrapeController
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

        [HttpGet("all")]
        public PagedResponse AllComments(string pageId, int pageNumber, int pageSize, OrderingType? order)
        {
            Func<QueryContainerDescriptor<ScrapedComment>, QueryContainer> search = q => q.Term(t => t.Field(c => c.Post.Id).Value(pageId));
            return CommentScraper.All(pageNumber, pageSize, p => p.CreatedTime, order, search);
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
