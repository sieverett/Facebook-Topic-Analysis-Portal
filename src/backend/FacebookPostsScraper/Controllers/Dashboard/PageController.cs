using System;
using System.Collections.Generic;
using System.Linq;
using Facebook;
using Facebook.Models;
using Facebook.Requests;
using FacebookCivicInsights.Data;
using FacebookCivicInsights.Models;
using Microsoft.AspNetCore.Mvc;

namespace FacebookCivicInsights.Controllers.Dashboard
{
    [Route("/api/dashboard/page")]
    public class PageController
    {
        private GraphClient GraphClient { get; }
        private ElasticSearchRepository<PageMetadata> PageRepository { get; }

        public PageController(GraphClient graphClient, ElasticSearchRepository<PageMetadata> pageRepository)
        {
            GraphClient = graphClient;
            PageRepository = pageRepository;
        }

        [HttpPost("new/multiple")]
        public IEnumerable<PageMetadata> AddMany([FromBody]PageMetadata[] pages)
        {
            if (pages == null)
            {
                throw new InvalidOperationException("No pages");
            }

            Console.WriteLine("Started creating {pages.Length} pages");

            for (int i = 0; i < pages.Length; i++)
            {
                PageMetadata page = pages[i];
                Console.WriteLine($"{i + 1}/{pages.Length}: {page.Name}");

                yield return Add(page);
            }

            Console.WriteLine("Done creating {pages.Length} pages");
        }

        [HttpPost("new")]
        public PageMetadata Add([FromBody]PageMetadata page)
        {
            DateTime now = DateTime.Now;

            // If the page doesn't already exist, save it.
            Page facebookPage = VerifyFacebookPage(page.FacebookId);
            if (PageRepository.Paged(search: q => q.Term("facebookId", page.FacebookId)).Data.Any())
            {
                throw new InvalidOperationException($"Page {page.FacebookId} already exists.");
            }

            page.Id = page.Name;
            page.FacebookId = facebookPage.Id;
            page.Category = facebookPage.Category;
            page.FirstScrape = now;
            page.LatestScrape = now;

            page.FanCountHistory.Add(new DatedFanCount
            {
                Date = now,
                FanCount = facebookPage.FanCount
            });

            page.Created = now;
            return PageRepository.Save(page);
        }

        [HttpGet("{id}")]
        public PageMetadata Get(string id) => PageRepository.Get(id);

        [HttpPatch("{id}")]
        public PageMetadata Update(string id, [FromBody]PageMetadata updatedPage)
        {
            if (updatedPage == null)
            {
                throw new InvalidOperationException("No updated page");
            }

            PageMetadata page = Get(id);
            if (page == null)
            {
                throw new InvalidOperationException("No such page");
            }

            page.Name = updatedPage.Name;
            page.FacebookId = VerifyFacebookPage(page.FacebookId).Id;

            return PageRepository.Save(page);
        }

        [HttpGet("all")]
        public PagedResponse AllPages(int pageNumber, int pageSize, bool? descending, DateTime? since, DateTime? until)
        {
            return PageRepository.All<TimeSearchResponse<PageMetadata>, PageMetadata>(
                new PagedResponse(pageNumber, pageSize),
                new Ordering<PageMetadata>("created", descending),
                p => p.Created, since, until);
        }

        [HttpDelete("{id}")]
        public PageMetadata Delete(string id) => PageRepository.Delete(id);

        private Page VerifyFacebookPage(string facebookId)
        {
            // Query the Facebook Graph API to make sur the page with the ID exists.
            Page page = GraphClient.GetPage<Page>(new PageRequest(facebookId));
            if (page == null)
            {
                throw new InvalidOperationException($"No such page {facebookId}.");
            }

            return page;
        }
    }
}
