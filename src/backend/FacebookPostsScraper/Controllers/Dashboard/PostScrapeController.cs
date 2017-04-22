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
using FacebookPostsScraper.Data.Translator;
using Microsoft.AspNetCore.Mvc;
using FacebookPostsScraper.Data.Scraper;
using System.Diagnostics;

namespace FacebookCivicInsights.Controllers.Dashboard
{
    [Route("/api/dashboard/post")]
    public class PostScrapeController : Controller
    {
        private PostScraper PostScraper { get; }
        private ElasticSearchRepository<ScrapedPage> PageScraper { get; }
        private ElasticSearchRepository<PostScrapeEvent> PostScrapeRepository { get; }

        public PostScrapeController(PostScraper postScraper, PageScraper pageScraper, ElasticSearchRepository<PostScrapeEvent> postScrapeRepository)
        {
            PostScraper = postScraper;
            PageScraper = pageScraper;
            PostScrapeRepository = postScrapeRepository;
        }

        [HttpGet("{id}")]
        public ScrapedPost GetPost(string id) => PostScraper.Get(id);

        [HttpGet("all")]
        public PagedResponse AllPosts(int pageNumber, int pageSize, OrderingType? order, DateTime? since, DateTime? until)
        {
            return PostScraper.All(pageNumber, pageSize, p => p.CreatedTime, order, p => p.CreatedTime, since, until);
        }

        [HttpGet("export")]
        public IActionResult ExportPost(OrderingType? order, DateTime? since, DateTime? until)
        {
            byte[] serialized = PostScraper.Export(p => p.CreatedTime, order, p => p.CreatedTime, since, until, CsvSerialization.MapPost);
            return File(serialized, "text/csv", "export.csv");
        }

        [HttpGet("scrape/{id}")]
        public PostScrapeEvent GetScrape(string id) => PostScrapeRepository.Get(id);

        [HttpGet("scrape/all")]
        public PagedResponse AllScrapes(int pageNumber, int pageSize, OrderingType? order, DateTime? since, DateTime? until)
        {
            return PostScrapeRepository.All(pageNumber, pageSize, p => p.ImportStart, order, p => p.ImportStart, since, until);
        }

        public class PostScrapeRequest
        {
            public IEnumerable<string> Pages { get; set; }
            public DateTime Since { get; set; }
            public DateTime Until { get; set; }
        }

        [HttpPost("scrape/scrape")]
        public PostScrapeEvent ScrapePosts([FromBody]PostScrapeRequest request)
        {
            Debug.Assert(request != null);

            // If no specific pages were specified, scrape them all.
            IEnumerable<ScrapedPage> pages;
            if (request.Pages == null)
            {
                pages = PageScraper.Paged().AllData();
            }
            else
            {
                pages = request.Pages.Select(p => PageScraper.Get(p));
            }

            ScrapedPost[] posts = PostScraper.Scrape(pages, request.Since, request.Until).ToArray();
            var postScrape = new PostScrapeEvent
            {
                Id = Guid.NewGuid().ToString(),
                Since = request.Since,
                Until = request.Until,
                ImportStart = posts.FirstOrDefault()?.Created ?? DateTime.Now,
                ImportEnd = DateTime.Now,
                NumberOfPosts = posts.Length,
                Pages = pages
            };

            return PostScrapeRepository.Save(postScrape);
        }

        [HttpGet("translate/{postId}")]
        public GoogleTranslatorResult Translate(string postId)
        {
            ScrapedPost post = GetPost(postId);
            if (post == null)
            {
                throw new InvalidOperationException($"No such post {postId}");
            }

            return new GoogleTranslator().Translate("km", "en", post.Message);
        }
    }
}
