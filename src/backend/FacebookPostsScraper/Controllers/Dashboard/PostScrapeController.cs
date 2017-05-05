using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using CsvHelper;
using Elasticsearch.Net;
using Facebook;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using FacebookCivicInsights.Data.Scraper;
using FacebookCivicInsights.Data.Importer;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        public PagedResponse<ScrapedPost> AllPosts(int pageNumber, int pageSize, string orderingKey, bool? descending, DateTime? since, DateTime? until)
        {
            return PostScraper.All<TimeSearchResponse<ScrapedPost>, ScrapedPost>(
                new PagedResponse(pageNumber, pageSize),
                new Ordering<ScrapedPost>(orderingKey ?? "created_time", descending),
                p => p.CreatedTime, since, until);
        }

        [HttpGet("export/csv")]
        public IActionResult ExportAsCSV(bool? descending, DateTime? since, DateTime? until)
        {
            IEnumerable<ScrapedPost> history = AllPosts(0, int.MaxValue, null, descending, since, until).AllData();

            byte[] serialized = CsvSerialization.Serialize(history, CsvSerialization.MapPost);
            return File(serialized, "text/csv", "export.csv");
        }

        [HttpGet("export/json")]
        public IActionResult ExportAsJson(bool? descending, DateTime? since, DateTime? until)
        {
            IEnumerable<ScrapedPost> history = AllPosts(0, int.MaxValue, null, descending, since, until).AllData();

            byte[] serialized = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(history));
            return File(serialized, "application/json-download", "export.json");
        }

        public class PostScrapeRequest
        {
            public IEnumerable<string> Pages { get; set; }
            public DateTime Since { get; set; }
            public DateTime Until { get; set; }
        }

        [HttpPost("scrape")]
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

            Console.WriteLine($"Started scraping comments for {posts.Length} posts");

            foreach (ScrapedPost post in posts)
            {
                ScrapedComment[] comments = CommentScraper.Scrape(post).ToArray();
                numberOfComments += comments.Length;
                Console.WriteLine(numberOfComments);
            }

            Console.WriteLine($"Done scraping {pages.Length} pages. Scraped {posts.Length} posts with {numberOfComments} comments");

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

        [HttpGet("import/historical")]
        public IEnumerable<ScrapedPost> ImportHistoricalPosts()
        {
            var importer = new ScrapeImporter(PageScraper, PageMetadataRepository, PostScraper);
            IEnumerable<string> files = Directory.EnumerateFiles("C:\\Users\\hughb\\Documents\\TAF\\Data", "*.csv", SearchOption.AllDirectories);
            IEnumerable<string> fanCountFiles = files.Where(f => f.Contains("DedooseChartExcerpts"));

            return importer.ImportPosts(fanCountFiles);
        }

        [HttpGet("import/elasticsearch")]
        public IEnumerable<ScrapedPost> ImportElasticSearchPosts(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            using (var streamReader = new StreamReader(fileStream))
            using (var csvReader = new CsvReader(streamReader))
            {
                csvReader.Configuration.RegisterClassMap<ScrapedPostMapping>();
                foreach (ScrapedPost record in csvReader.GetRecords<ScrapedPost>())
                {
                    yield return PostScraper.Save(record, Refresh.False);
                }
            }
        }

        [HttpGet("history/{id}")]
        public PostScrapeHistory GetScrape(string id) => PostScrapeHistoryRepository.Get(id);

        [HttpGet("history/all")]
        public PagedResponse AllScrapes(int pageNumber, int pageSize, bool? descending, DateTime? since, DateTime? until)
        {
            return PostScrapeHistoryRepository.All<TimeSearchResponse<PostScrapeHistory>, PostScrapeHistory>(
                new PagedResponse(pageNumber, pageSize),
                new Ordering<PostScrapeHistory>("importStart", descending),
                p => p.ImportStart, since, until);
        }

        [HttpGet("and_so_it_begins")]
        public void GoThroughEachPostAndGetTheCommentsOhMyGodThisWillDestroyMyLaptop()
        {
            const int LastScrapeAmount = 0;
            int i = 0;
            ElasticSearchPagedResponse<ScrapedPost> paged = PostScraper.Paged(new PagedResponse(0, int.MaxValue), new Ordering<ScrapedPost>("created_time", true));
            foreach (ElasticSearchPagedResponse<ScrapedPost> response in paged.AllPages())
            {
                foreach (ScrapedPost post in response.Data)
                {
                    i++;
                    if (post.CreatedTime < new DateTime(2017, 04, 01))
                    {
                        continue;
                    }
                    if (i > LastScrapeAmount)
                    {
                        List<ScrapedComment> comments = CommentScraper.Scrape(post).ToList();
                        Console.WriteLine($"{i}/{response.TotalCount}: {post.Id}; {comments.Count}");
                    }
                    else
                    {
                        Console.WriteLine($"{i}/{response.TotalCount}: {post.Id}; Already scraped.");
                    }
                }
            }
        }
    }
}
