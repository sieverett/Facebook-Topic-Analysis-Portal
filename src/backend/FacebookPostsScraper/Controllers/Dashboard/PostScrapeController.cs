using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Facebook;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using FacebookPostsScraper.Data;
using Microsoft.AspNetCore.Mvc;
using FacebookPostsScraper.Data.Scraper;
using FacebookPostsScraper.Data.Importer;
using System.IO;

namespace FacebookCivicInsights.Controllers.Dashboard
{
    [Route("/api/dashboard/scrape/post")]
    public class PostScrapeController : Controller
    {
        private PostScraper PostScraper { get; }
        private CommentScraper CommentScraper { get; }
        private PageScraper PageScraper { get; }
        private ElasticSearchRepository<PageMetadata> PageMetadataRepository { get; }
        private ElasticSearchRepository<PostScrapeHistory> PostScrapeHistoryRepository { get; }

        public PostScrapeController(PostScraper postScraper, CommentScraper commentScraper, PageScraper pageScraper, ElasticSearchRepository<PageMetadata> pageMetadataRepository, ElasticSearchRepository<PostScrapeHistory> postScrapeHistoryRepository)
        {
            PostScraper = postScraper;
            CommentScraper = commentScraper;
            PageScraper = pageScraper;
            PageMetadataRepository = pageMetadataRepository;
            PostScrapeHistoryRepository = postScrapeHistoryRepository;
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
        public PostScrapeHistory GetScrape(string id) => PostScrapeHistoryRepository.Get(id);

        [HttpGet("scrape/all")]
        public PagedResponse AllScrapes(int pageNumber, int pageSize, OrderingType? order, DateTime? since, DateTime? until)
        {
            return PostScrapeHistoryRepository.All(pageNumber, pageSize, p => p.ImportStart, order, p => p.ImportStart, since, until);
        }

        public class PostScrapeRequest
        {
            public IEnumerable<string> Pages { get; set; }
            public DateTime Since { get; set; }
            public DateTime Until { get; set; }
        }

        [HttpPost("scrape/scrape")]
        public PostScrapeHistory ScrapePosts([FromBody]PostScrapeRequest request)
        {
            Debug.Assert(request != null);
            Console.WriteLine("Started Scraping");

            // If no specific pages were specified, scrape them all.
            PageMetadata[] pages;
            if (request.Pages == null)
            {
                pages = PageMetadataRepository.Paged().AllData().ToArray();
            }
            else
            {
                pages = request.Pages.Select(p => PageMetadataRepository.Get(p)).ToArray();
            }

            int numberOfComments = 0;
            ScrapedPost[] posts = PostScraper.Scrape(pages, request.Since, request.Until).ToArray();
            foreach (ScrapedPost post in posts)
            {
                ScrapedComment[] comments = CommentScraper.Scrape(post).ToArray();
                numberOfComments += comments.Length;
                Console.WriteLine(numberOfComments);
            }
            Console.WriteLine("Done Scraping");

            var postScrape = new PostScrapeHistory
            {
                Id = Guid.NewGuid().ToString(),
                Since = request.Since,
                Until = request.Until,
                ImportStart = posts.FirstOrDefault()?.Scraped ?? DateTime.Now,
                ImportEnd = DateTime.Now,
                NumberOfPosts = posts.Length,
                NumberOfComments = numberOfComments,
                Pages = pages
            };

            return PostScrapeHistoryRepository.Save(postScrape);
        }

        [HttpGet("import")]
        public IEnumerable<ScrapedPost> ImportPages()
        {
            var importer = new ScrapeImporter(PageScraper, PostScraper);
            IEnumerable<string> files = Directory.EnumerateFiles("C:\\Users\\hughb\\Documents\\TAF\\Data", "*.csv", SearchOption.AllDirectories);
            IEnumerable<string> fanCountFiles = files.Where(f => f.Contains("DedooseChartExcerpts"));

            return importer.ImportPosts(fanCountFiles);
        }
    }
}
