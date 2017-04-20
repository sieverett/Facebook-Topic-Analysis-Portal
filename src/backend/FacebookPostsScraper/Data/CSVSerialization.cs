using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using FacebookCivicInsights.Models;

namespace FacebookPostsScraper.Data
{
    public static class CsvSerialization
    {
        public static byte[] Serialize<T>(IEnumerable<T> data, Func<T, dynamic> mappingFactory)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
                using (var csvWriter = new CsvWriter(streamWriter))
                {
                    csvWriter.WriteRecords(data.Select(p => mappingFactory(p)));
                }

               return memoryStream.ToArray();
            }
        }

        public static dynamic MapPost(ScrapedPost post)
        {
            dynamic expanded = new ExpandoObject();

            expanded.Id = post.Id;
            expanded.Message = post.Message;
            expanded.Link = post.Link;
            expanded.CreatedTime = post.CreatedTime;
            expanded.UpdatedTime = post.UpdatedTime;
            expanded.StatusType = post.StatusType;
            expanded.Type = post.Type;
            expanded.Permalink = post.Permalink;
            expanded.Reactions = post.Reactions.Summary.TotalCount;
            expanded.Comments = post.Comments.Summary.TotalCount;
            expanded.Shares = post.Shares.Count;
            expanded.pageName = post.Page.Name;
            expanded.pageId = post.Page.FacebookId;
            expanded.pageLikes = post.Page.FanCount;

            return expanded;
        }

        public static dynamic MapPageScrape(PageScrapeEvent scrape)
        {
            dynamic expanded = new ExpandoObject();
            IDictionary<string, object> expandedDictionary = (IDictionary<string, object>)expanded;

            expanded.Date = scrape.ImportStart;

            foreach (ScrapedPage page in scrape.Pages)
            {
                expandedDictionary.Add(page.Name, page.FanCount);
            }

            return expanded;
        }
    }
}
