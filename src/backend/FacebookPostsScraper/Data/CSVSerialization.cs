using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using FacebookCivicInsights.Models;
using Facebook.Models;

namespace FacebookPostsScraper.Data
{
    public static class CsvSerialization
    {
        public static byte[] Serialize<T>(IEnumerable<T> data, Func<T, dynamic> mappingFactory)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(streamWriter))
            {
                csvWriter.WriteRecords(data.Select(p => mappingFactory(p)));

                streamWriter.Flush();
                return memoryStream.ToArray();
            }
        }

        public static dynamic MapPost(ScrapedPost post)
        {
            // Flatten out the post and export it.
            dynamic flattened = new ExpandoObject();

            flattened.Id = post.Id;
            flattened.Message = post.Message;

            flattened.Topics = string.Join(",", post.Topics);

            flattened.Link = post.Link;
            flattened.Caption = post.Caption;
            flattened.Description = post.Description;
            flattened.Permalink = post.Permalink;

            flattened.CreatedTime = post.CreatedTime;
            flattened.UpdatedTime = post.UpdatedTime;

            flattened.Name = post.Name;
            flattened.StatusType = post.StatusType;
            flattened.Type = post.Type;

            flattened.Reactions = post.Reactions.Summary.TotalCount;
            flattened.Comments = post.Comments.Summary.TotalCount;
            flattened.Shares = post.Shares.Count;

            flattened.InternalPageId = post.Page.Id;
            flattened.PageId = post.Page.FacebookId;
            flattened.PageName = post.Page.Name;
            flattened.PageCategory = post.Page.Category;
            flattened.PageLikes = post.Page.FanCount;
            flattened.PageLikesDate = post.Page.Date;

            flattened.PosterId = post.Poster?.Id;
            flattened.PosterName = post.Poster?.Name;

            flattened.GeoPoint = post.GeoPoint;

            flattened.PlaceId = post.Place?.Id;
            flattened.PlaceName = post.Place?.Name;
            flattened.PlaceLocationCity = post.Place?.Location?.City;
            flattened.PlaceLocationRegion = post.Place?.Location?.Region;
            flattened.PlaceLocationCountry = post.Place?.Location?.Country;
            flattened.PlaceLocationLatitude = post.Place?.Location?.Latitude;
            flattened.PlaceLocationLongitude = post.Place?.Location?.Longitude;

            flattened.Scraped = post.Scraped;
            flattened.LastScraped = post.LastScraped;

            return flattened;
        }

        public static dynamic MapPageScrape(PageScrapeHistory scrape)
        {
            dynamic expanded = new ExpandoObject();

            // TODO

            IDictionary<string, object> expandedDictionary = (IDictionary<string, object>)expanded;

            expanded.Date = scrape.ImportStart;

            foreach (PageMetadata page in scrape.Pages)
            {
                expandedDictionary.Add(page.Name, page.FanCount);
            }

            return expanded;
        }
    }

    public class ScrapedPostMapping : CsvClassMap<ScrapedPost>
    {
        public ScrapedPostMapping()
        {
            // Read the flattened out post.
            Map(p => p.Id);
            Map(p => p.Message);

            Map(p => p.Topics).ConvertUsing(row =>
            {
                return row.GetField("Topics").Split(',');
            });

            Map(p => p.Link);
            Map(p => p.Caption);
            Map(p => p.Description);
            Map(p => p.Permalink);

            Map(p => p.CreatedTime);
            Map(p => p.UpdatedTime);

            Map(p => p.Name);
            Map(p => p.StatusType);
            Map(p => p.Type);

            Map(p => p.Reactions).ConvertUsing(row =>
            {
                return new Reactions { Summary = new ReactionsSummary { TotalCount = row.GetField<int>("Reactions") } };
            });

            Map(p => p.Comments).ConvertUsing(row =>
            {
                return new Comments { Summary = new CommentsSummary { TotalCount = row.GetField<int>("Comments") } };
            });

            Map(p => p.Shares).ConvertUsing(row =>
            {
                return new Shares { Count = row.GetField<int>("Comments") };
            });

            Map(p => p.Page).ConvertUsing(row =>
            {
                return new ScrapedPage
                {
                    Id = row.GetField("InternalPageId"),
                    FacebookId = row.GetField("PageId"),
                    Name = row.GetField("PageName"),
                    Category = row.GetField("PageCategory"),
                    FanCount = row.GetField<int>("PageLikes"),
                    Date = row.GetField<DateTime>("PageLikesDate")
                };
            });

            Map(p => p.Poster).ConvertUsing(row =>
            {
                return new Profile
                {
                    Id = row.GetField("PosterId"),
                    Name = row.GetField("PosterName")
                };
            });

            Map(p => p.GeoPoint);

            Map(p => p.Place).ConvertUsing(row =>
            {
                return new Place
                {
                    Id = row.GetField("PlaceId"),
                    Name = row.GetField("PlaceName"),
                    Location = new Location
                    {
                        City = row.GetField("PlaceLocationCity"),
                        Region = row.GetField("PlaceLocationRegion"),
                        Country = row.GetField("PlaceLocationCountry"),
                        Latitude = row.GetField("PlaceLocationLatitude"),
                        Longitude = row.GetField("PlaceLocationLongitude")
                    }
                };
            });

            Map(p => p.Scraped);
            Map(p => p.LastScraped);
        }
    }
}
