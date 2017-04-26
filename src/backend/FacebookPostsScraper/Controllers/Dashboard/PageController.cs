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
        public IEnumerable<PageMetadata> AddMany([FromBody]IEnumerable<PageMetadata> pages)
        {
            if (pages == null)
            {
                throw new InvalidOperationException("No pages");
            }

            foreach (PageMetadata page in pages)
            {
                Console.WriteLine(page.Name);
                yield return Add(page);
            }
        }

        [HttpPost("new")]
        public PageMetadata Add([FromBody]PageMetadata page)
        {
            DateTime now = DateTime.Now;

            // If the page doesn't already exist, save it.
            Page facebookPage = VerifyFacebookPage(page.FacebookId);
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
        public PagedResponse AllPages(int pageNumber, int pageSize, OrderingType? order, DateTime? since, DateTime? until)
        {
            return PageRepository.All<TimeSearchResponse<PageMetadata>, PageMetadata>(
                pageNumber,
                pageSize,
                p => p.Created,
                order,
                p => p.Created,
                since,
                until);
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
            if (PageRepository.Paged(search: q => q.Term("facebookId", page.Id)).Data.Any())
            {
                throw new InvalidOperationException($"Page {facebookId} already exists.");
            }

            return page;
        }
    }
}
