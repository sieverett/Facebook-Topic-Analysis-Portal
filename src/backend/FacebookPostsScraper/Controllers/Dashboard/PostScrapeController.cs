using Facebook;
using Facebook.Requests;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using FacebookPostsScraper.Data.Translator;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FacebookCivicInsights.Controllers.Dashboard
{
    [Route("/api/dashboard/post")]
    public class PostScrapeController : Controller
    {
        private GraphClient GraphClient { get; }
        private IDataRepository<ScrapedPost> PostRepository { get; }
        private IDataRepository<ScrapedPage> PageRepository { get; }
        private IDataRepository<PostScrapeEvent> PostScrapeRepository { get; }

        public PostScrapeController(
            GraphClient graphClient,
            IDataRepository<ScrapedPost> postRepository,
            IDataRepository<ScrapedPage> pageRepository,
            IDataRepository<PostScrapeEvent> postScrapeRepository)
        {
            GraphClient = graphClient;
            PostRepository = postRepository;
            PageRepository = pageRepository;
            PostScrapeRepository = postScrapeRepository;
        }

        [HttpGet("{id}")]
        public ScrapedPost GetPost(string id) => PostRepository.Get(id);

        [HttpGet("all")]
        public IActionResult AllPosts(int pageNumber, int pageSize, OrderingType? order)
        {
            var paging = new PagedResponse { PageNumber = pageNumber, PageSize = pageSize };
            var ordering = new Ordering<ScrapedPost>
            {
                Order = order ?? OrderingType.Descending,
                Path = p => p.CreatedTime
            };

            PagedResponse<ScrapedPost> content = PostRepository.Paged(paging, ordering);
            return Ok(content);
        }

        [HttpGet("scrape/{id}")]
        public PostScrapeEvent GetScrape(string id) => PostScrapeRepository.Get(id);

        [HttpGet("scrape/all")]
        public IActionResult AllScrapes(int pageNumber, int pageSize, OrderingType? order)
        {
            var paging = new PagedResponse { PageNumber = pageNumber, PageSize = pageSize };
            var ordering = new Ordering<PostScrapeEvent>
            {
                Order = order ?? OrderingType.Descending,
                Path = p => p.ImportStart
            };

            PagedResponse<PostScrapeEvent> content = PostScrapeRepository.Paged(paging, ordering);
            return Ok(content);
        }

        public class PostScrapeRequest
        {
            public DateTime Since { get; set; }
            public DateTime Until { get; set; }
            public IEnumerable<string> Pages { get; set; }
        }

        [HttpPost("scrape/scrape")]
        public PostScrapeEvent ScrapePosts([FromBody]PostScrapeRequest request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("No request");
            }
            if (request.Since == DateTime.MinValue || request.Until == DateTime.MaxValue)
            {
                throw new InvalidOperationException("Invalid since or until");
            }
            if (request.Since >= request.Until)
            {
                throw new InvalidOperationException("Since greater than or equal to until");
            }
            if (request.Since >= DateTime.Now || request.Until >= DateTime.Now)
            {
                throw new InvalidOperationException("Since or until greater than or equal to now");
            }

            // If no specific pages were specified, scrape them all.
            IEnumerable<ScrapedPage> pages;
            if (request.Pages == null)
            {
                pages = PageRepository.Paged().AllData().Flatten();
            }
            else
            {
                pages = request.Pages.Select(p => PageRepository.Get(p));
            }

            DateTime start = DateTime.Now;
            int numberOfPosts = 0;

            foreach (ScrapedPage page in pages)
            {
                // Query the Facebook Graph API to get all posts in the given range, published only by
                // the page.
                var graphRequest = new PostsRequest
                {
                    PageId = page.FacebookId,
                    Since = request.Since,
                    Until = request.Until,
                    Edge = PostsRequestEdge.Posts
                };

                PagedResponse<ScrapedPost> postsResponse = GraphClient.GetPosts<ScrapedPost>(graphRequest);
                foreach (ScrapedPost post in postsResponse.AllData().Flatten())
                {
                    // Update the database with the new post.
                    post.Page = page;
                    post.Created = DateTime.Now;
                    post.LastScraped = start;
                    PostRepository.Save(post);

                    numberOfPosts++;
                }

                // Don't store the entire fan count history for the page belonging to each post.
                page.FanCountHistory = page.FanCountHistory.Take(1).ToList();
            }

            var postScrape = new PostScrapeEvent
            {
                Id = Guid.NewGuid().ToString(),
                Since = request.Since,
                Until = request.Until,
                ImportStart = start,
                ImportEnd = DateTime.Now,
                NumberOfPosts = numberOfPosts,
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
