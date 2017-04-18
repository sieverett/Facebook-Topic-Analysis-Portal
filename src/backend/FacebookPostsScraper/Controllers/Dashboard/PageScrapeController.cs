using Facebook;
using Facebook.Models;
using Facebook.Requests;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FacebookCivicInsights.Controllers.Dashboard
{
    [Route("/api/dashboard/page")]
    public class PageScrapeController : Controller
    {
        private GraphClient GraphClient { get; }
        private IDataRepository<ScrapedPage> PageRepository { get; }
        private IDataRepository<PageScrapeEvent> PageScrapeRepository { get; }

        public PageScrapeController(
            GraphClient graphClient,
            IDataRepository<ScrapedPage> pageRepository,
            IDataRepository<PageScrapeEvent> pageScrapeRepository)
        {
            GraphClient = graphClient;
            PageRepository = pageRepository;
            PageScrapeRepository = pageScrapeRepository;
        }

        [HttpPost("new/multiple")]
        public IEnumerable<ScrapedPage> AddMany([FromBody]IEnumerable<ScrapedPage> pages)
        {
            if (pages == null)
            {
                throw new InvalidOperationException("No pages");
            }

            foreach (ScrapedPage page in pages)
            {
                yield return Add(page);
            }
        }

        [HttpPost("new")]
        public ScrapedPage Add([FromBody]ScrapedPage page)
        {
            DateTime now = DateTime.Now;

            // Get the Facebook page and save the page.s
            Page facebookPage = VerifyFacebookPage(page.FacebookId);
            page.Id = Guid.NewGuid().ToString();
            page.FacebookId = facebookPage.Id;
            page.FirstScrape = now;
            page.LatestScrape = now;

            page.FanCountHistory.Add(new DatedFanCount
            {
                Date = now,
                FanCount = facebookPage.FanCount
            });

            page.Created = now;
            PageRepository.Save(page);

            return page;
        }

        [HttpGet("{id}")]
        public ScrapedPage Get(string id) => PageRepository.Get(id);

        [HttpPatch("{id}")]
        public ScrapedPage Update(string id, [FromBody]ScrapedPage updatedPage)
        {
            if (updatedPage == null)
            {
                throw new InvalidOperationException("No updated page");
            }

            ScrapedPage page = Get(id);
            if (page == null)
            {
                throw new InvalidOperationException("No such page");
            }

            page.Name = updatedPage.Name;
            page.FacebookId = VerifyFacebookPage(page.FacebookId).Id;

            return PageRepository.Save(page);
        }

        private Page VerifyFacebookPage(string facebookId)
        {
            // Query the Facebook Graph API to make sur the page with the ID exists.
            Page facebookPage = GraphClient.GetPage(new PageRequest { PageId = facebookId });
            if (facebookPage == null)
            {
                throw new InvalidOperationException($"No such page {facebookId}");
            }

            var facebookIdSearch = new Search<ScrapedPage>
            {
                Field = p => p.FacebookId,
                Value = facebookId
            };
            if (PageRepository.Paged(search: facebookIdSearch).Data.Any())
            {
                throw new InvalidOperationException($"Page already exists {facebookId}");
            }

            return facebookPage;
        }

        [HttpGet("all")]
        public IActionResult Browse(int pageNumber, int pageSize, OrderingType? order)
        {
            var paging = new PagedResponse { PageNumber = pageNumber, PageSize = pageSize };
            var ordering = new Ordering<ScrapedPage>
            {
                Order = order ?? OrderingType.Descending,
                Path = p => p.Created
            };

            PagedResponse<ScrapedPage> content = PageRepository.Paged(paging, ordering);
            return Ok(content);
        }

        [HttpDelete("{id}")]
        public ScrapedPage Delete(string id) => PageRepository.Delete(id);

        [HttpGet("scrape/{id}")]
        public PageScrapeEvent GetScrape(string id) => PageScrapeRepository.Get(id);

        [HttpGet("scrape/all")]
        public IActionResult AllScrapes(int pageNumber, int pageSize, OrderingType? order)
        {
            var paging = new PagedResponse { PageNumber = pageNumber, PageSize = pageSize };
            var ordering = new Ordering<PageScrapeEvent>
            {
                Order = order ?? OrderingType.Descending,
                Path = p => p.ImportStart
            };

            PagedResponse<PageScrapeEvent> content = PageScrapeRepository.Paged(paging, ordering);
            return Ok(content);
        }

        [HttpPost("scrape/scrape")]
        public PageScrapeEvent ScrapePages([FromBody]IEnumerable<string> request)
        {
            // If no specific pages were specified, scrape them all.
            IEnumerable<ScrapedPage> pages;
            if (request == null)
            {
                pages = PageRepository.Paged().AllData().Flatten();
            }
            else
            {
                pages = request.Select(id => Get(id));
            }

            DateTime start = DateTime.Now;
            foreach (ScrapedPage page in pages)
            {
                if (page == null)
                {
                    throw new InvalidOperationException("No such page in database {page.Name}");
                }

                // Query the Facebook Graph API to get the page likes.
                Page facebookPage = GraphClient.GetPage(new PageRequest { PageId = page.FacebookId });
                if (facebookPage == null)
                {
                    throw new InvalidOperationException($"No such facebook page {page.Name}");
                }

                // Update the database with the new information.
                page.FirstScrape = page.FirstScrape ?? start;
                page.LatestScrape = start;
                page.FanCountHistory.Insert(0, new DatedFanCount
                {
                    Date = start,
                    FanCount = facebookPage.FanCount
                });

                PageRepository.Save(page);

                // Don't store the entire fan count history for each page on each scrape.
                page.FanCountHistory = new List<DatedFanCount> { page.FanCountHistory.First() };
            }

            var pageScrapeEvent = new PageScrapeEvent
            {
                Id = Guid.NewGuid().ToString(),
                ImportStart = start,
                ImportEnd = DateTime.Now,
                Pages = pages
            };

            return PageScrapeRepository.Save(pageScrapeEvent);
        }
    }
}
