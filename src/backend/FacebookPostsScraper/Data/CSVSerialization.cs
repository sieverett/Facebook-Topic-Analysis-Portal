using CsvHelper;
using CsvHelper.Configuration;
using FacebookCivicInsights.Models;
using System.Collections;
using System.IO;
using System.Text;

namespace FacebookPostsScraper.Data
{
    public static class CsvSerialization
    {
        public static MemoryStream Serialize<TMapping>(IEnumerable data) where TMapping : CsvClassMap
        {
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);

            var csv = new CsvWriter(streamWriter);
            csv.Configuration.RegisterClassMap<TMapping>();

            csv.WriteRecords(data);

            memoryStream.Position = 0;
            return memoryStream;
        }
    }

    public sealed class ScrapedPostMapping : CsvClassMap<ScrapedPost>
    {
        public ScrapedPostMapping()
        {
            Map(p => p.Id);
            Map(p => p.Message);
            Map(p => p.Link);
            Map(p => p.CreatedTime);
            Map(p => p.UpdatedTime);
            Map(p => p.StatusType);
            Map(p => p.Type);
            Map(p => p.Permalink);
            Map(p => p.Reactions.Summary.TotalCount).Name("Reactions");
            Map(p => p.Comments.Summary.TotalCount).Name("Comments");
            Map(p => p.Shares.Count).Name("Shares");
            Map(p => p.Permalink);
            Map(p => p.Page.Name).Name("pageName");
            Map(p => p.Page.FacebookId).Name("pageId");
            Map(p => p.Page.FanCount).Name("pageLikes");
        }
    }
}
