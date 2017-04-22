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

namespace FacebookCivicInsights.Controllers.Dashboard
{
    [Route("/api/dashboard/comment")]
    public class CommentScrapeController : Controller
    {
        private GraphClient GraphClient { get; }
        private ElasticSearchRepository<ScrapedPage> PageRepository { get; }
        private ElasticSearchRepository<ScrapedComment> CommentRepository { get; }

        public CommentScrapeController(
            GraphClient graphClient,
            ElasticSearchRepository<ScrapedPage> pageRepository,
            ElasticSearchRepository<ScrapedComment> commentRepository)
        {
            GraphClient = graphClient;
            PageRepository = pageRepository;
            CommentRepository = commentRepository;
        }

        [HttpGet("{id}")]
        public Comment GetComment(string id) => CommentRepository.Get(id);

        [HttpGet("all")]
        public PagedResponse AllComments(string pageId, int pageNumber, int pageSize, OrderingType? order)
        {
            return CommentRepository.All(pageNumber, pageSize, p => p.CreatedTime, order, p => p.CreatedTime, null, null);
        }

        public class CommentScrapeRequest
        {
            public string PostId { get; set; }
        }

        [HttpPost("scrape")]
        public IEnumerable<ScrapedComment> ScrapeComments([FromBody]CommentScrapeRequest request)
        {
            if (request?.PostId == null)
            {
                throw new InvalidOperationException("No request");
            }

            DateTime now = DateTime.Now;
            CommentsRequest graphRequest = new CommentsRequest(request.PostId);
            foreach (ScrapedComment comment in GraphClient.GetComments<ScrapedComment>(graphRequest))
            {
                if (comment.Created == DateTime.MinValue)
                {
                    comment.Created = now;
                }
                comment.LastScraped = now;
                comment.ParentId = request.PostId;

                CommentRepository.Save(comment, Refresh.False);

                yield return comment;
            }
        }
    }
}
