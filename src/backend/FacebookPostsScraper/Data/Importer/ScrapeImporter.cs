using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using FacebookCivicInsights.Models;
using FacebookPostsScraper.Data.Scraper;
using Elasticsearch.Net;
using Facebook.Models;

namespace FacebookPostsScraper.Data.Importer
{
    public class ScrapeImporter
    {
        private PageScraper PageScraper { get; }
        private PostScraper PostScraper { get; }

        public ScrapeImporter(PageScraper pageScraper, PostScraper postScraper)
        {
            PageScraper = pageScraper;
            PostScraper = postScraper;
        }

        // Yuck: each page has a number of different names in the data.
        private static Dictionary<string, ScrapedPage> Mappings { get; } = new Dictionary<string, ScrapedPage>(StringComparer.OrdinalIgnoreCase)
        {
            { "hun sen", new ScrapedPage { DisplayName = "Hun Sen", FacebookId = "hunsencambodia" } },

            { "sam rainsy", new ScrapedPage { DisplayName = "Sam Rainsy", FacebookId = "rainsy.sam.5" } },
            { "samrainsy",  new ScrapedPage { DisplayName = "Sam Rainsy", FacebookId = "rainsy.sam.5" } },

            { "cambodian people's party", new ScrapedPage { DisplayName = "Cambodian People's Party", FacebookId = "lovepeoplekh.com.kh" } },
            { "ccp",                      new ScrapedPage { DisplayName = "Cambodian People's Party", FacebookId = "lovepeoplekh.com.kh" } },
            { "cpp",                      new ScrapedPage { DisplayName = "Cambodian People's Party", FacebookId = "lovepeoplekh.com.kh" } },

            { "cambodia national rescue party", new ScrapedPage { DisplayName = "Cambodia National Resuce Party", FacebookId = "CNRParty" } },
            { "cnrp",                           new ScrapedPage { DisplayName = "Cambodia National Resuce Party", FacebookId = "CNRParty" } },

            { "voice of america khmer", new ScrapedPage { DisplayName = "Voice of America Khmer", FacebookId = "voakhmer" } },
            { "voakhmer",               new ScrapedPage { DisplayName = "Voice of America Khmer", FacebookId = "voakhmer" } },
            { "voa khmer",              new ScrapedPage { DisplayName = "Voice of America Khmer", FacebookId = "voakhmer" } },

            { "post khmer", new ScrapedPage { DisplayName = "Post Khmer", FacebookId = "PostKhmer" } },

            { "radio free asia khmer", new ScrapedPage { DisplayName = "Radio Free Asia Khmer", FacebookId = "rfacambodia" } },
            { "rfa",                   new ScrapedPage { DisplayName = "Radio Free Asia Khmer", FacebookId = "rfacambodia" } },
            { "rfa khmer",             new ScrapedPage { DisplayName = "Radio Free Asia Khmer", FacebookId = "rfacambodia" } },
            { "rfa cambodia",          new ScrapedPage { DisplayName = "Radio Free Asia Khmer", FacebookId = "rfacambodia" } },

            { "radio france", new ScrapedPage { DisplayName = "Radio France", FacebookId = "RFIKhmer" } },
            { "rfi khmer",    new ScrapedPage { DisplayName = "Radio France", FacebookId = "RFIKhmer" } },
            { "rfi",          new ScrapedPage { DisplayName = "Radio France", FacebookId = "RFIKhmer" } },

            { "vodhotnews", new ScrapedPage { DisplayName = "VoDHotNews", FacebookId = "VODKhmer" } },
            { "vod",        new ScrapedPage { DisplayName = "VoDHotNews", FacebookId = "VODKhmer" } },
            { "vod khmer",  new ScrapedPage { DisplayName = "VoDHotNews", FacebookId = "VODKhmer" } },

            { "camnews", new ScrapedPage { DisplayName = "Camnews", FacebookId = "camnews.org" } },

            { "moeys",                                    new ScrapedPage { DisplayName = "Moey", FacebookId = "moeys.gov.kh" } },
            { "moey",                                     new ScrapedPage { DisplayName = "Moey", FacebookId = "moeys.gov.kh" } },
            { "ministry of education, youth and spots",   new ScrapedPage { DisplayName = "Moey", FacebookId = "moeys.gov.kh" } },
            { "Ministry of Education, Youth, and Sports", new ScrapedPage { DisplayName = "Moey", FacebookId = "moeys.gov.kh" } },

            { "anti-corruption unit", new ScrapedPage { DisplayName = "Anti-corruption Unit", FacebookId = "acukhmer" } },
            { "acu",                  new ScrapedPage { DisplayName = "Anti-corruption Unit", FacebookId = "acukhmer" } },

            { "kem sokha", new ScrapedPage { DisplayName = "Kem Sokha", FacebookId = "kemsokha" } },

            { "hun many", new ScrapedPage { DisplayName = "Hun Many", FacebookId = "151797291581699" } },

            { "ministry of commerce", new ScrapedPage { DisplayName = "Ministry of Commerce", FacebookId = "moc.gov.kh" } },
            { "mocommerce",           new ScrapedPage { DisplayName = "Ministry of Commerce", FacebookId = "moc.gov.kh" } },

            { "cambodia express news",  new ScrapedPage { DisplayName = "Cambodia Express News", FacebookId = "cen.com.kh" } },
            { "cambodian express news", new ScrapedPage { DisplayName = "Cambodia Express News", FacebookId = "cen.com.kh" } }
        };

        public IEnumerable<ScrapedPage> ImportPages(IEnumerable<string> fanCountCSVs)
        {
            DateTime now = DateTime.Now;
            int numberSaved = 0;
            foreach (string fanCountCSV in fanCountCSVs)
            {
                // Fire up the CSV reader.
                using (var fileStream = new FileStream(fanCountCSV, FileMode.Open))
                using (var streamReader = new StreamReader(fileStream))
                using (var csvReader = new CsvReader(streamReader))
                {
                    var records = csvReader.GetRecords<dynamic>();
                    foreach (IDictionary<string, object> record in records)
                    {
                        // The date is a string in a 2016-12-25 format.
                        DateTime date = DateTime.ParseExact((string)record["Dates"], "yyyy-MM-dd", null);

                        // Now get the list of all the pages.
                        foreach (string pageName in record.Keys)
                        {
                            // Skip all columns that are empty or are the "Dates" field.
                            if (pageName == "" || pageName == "Dates")
                            {
                                continue;
                            }

                            // Yuck: page names have varying degrees of leading and trailing whitespace.
                            // Yuck: page names for the same page vary between instances.
                            ScrapedPage mappedPage = Mappings[pageName.Trim()];
                            if (mappedPage.Name == null)
                            {
                                // Add the page to the list if the page doesn't already exist.
                                Console.WriteLine($"Creating {mappedPage.DisplayName}");
                                ScrapedPage facebookPage = PageScraper.Scrape(mappedPage.FacebookId, false, now);
                                mappedPage.FacebookId = facebookPage.FacebookId;
                                mappedPage.Name = facebookPage.Name;
                                mappedPage.Category = facebookPage.Category;
                            }

                            // Now get the number of likes from the table.
                            // Yuck: some data is missing, or contains letters in.
                            // Yuck: some full numbers have decimal points in.
                            string numberOfLikesAsString = (string)record[pageName];
                            if (!int.TryParse(numberOfLikesAsString, NumberStyles.AllowDecimalPoint, null, out int numberOfLikes))
                            {
                                // If we can't parse the number of likes as an actual number, skip it.
                                Console.WriteLine("Can't parse number of likes");
                                continue;
                            }

                            // Add this to the fan count history.
                            ScrapedPage savedPage = PageScraper.Closest(mappedPage.DisplayName, date);
                            if (savedPage == null || savedPage.Date != date)
                            {
                                // Page doesn't have this date already. Add it.
                                savedPage = mappedPage;
                                savedPage.Id = Guid.NewGuid().ToString();
                                savedPage.Date = date;
                                savedPage.FanCount = numberOfLikes;
                            }
                            else
                            {
                                // Page already has this date already. Update it.
                                savedPage.FanCount = numberOfLikes;
                            }

                            // Save the page.
                            numberSaved++;
                            Console.WriteLine(numberSaved);
                            yield return PageScraper.Save(savedPage, Refresh.False);

                            // Make sure metadata set now doesn't pass on.
                            mappedPage.Id = null;
                            mappedPage.Date = DateTime.MinValue;
                            mappedPage.FanCount = 0;
                        }
                    }
                }
            }
        }

        public IEnumerable<ScrapedPost> ImportPosts(IEnumerable<string> postCSVs)
        {
            DateTime now = DateTime.Now;
            int numberSaved = 0;
            foreach (string postCSV in postCSVs)
            {
                // Fire up the CSV reader.
                using (var fileStream = new FileStream(postCSV, FileMode.Open))
                using (var streamReader = new StreamReader(fileStream))
                using (var csvReader = new CsvReader(streamReader))
                {
                    var records = csvReader.GetRecords<dynamic>();
                    foreach (IDictionary<string, object> record in records)
                    {
                        string postId = (string)record["Media Title"];
                        ScrapedPost savedPost = PostScraper.Get(postId);
                        if (savedPost != null)
                        {
                            // Skip posts that already exist.
                            //Console.WriteLine($"Skipping {postId}.");
                            //continue;
                        }

                        ScrapedPost post = PostScraper.ScrapePost(postId);
                        bool useDatabase = post == null;
                        if (post == null)
                        {
                            // Post has been deleted - we still want to save it..
                            post = new ScrapedPost { Id = postId };
                            Console.WriteLine($"Post {postId} does not exist.");
                        }

                        string normalizedPageName = null;
                        foreach (string field in record.Keys)
                        {
                            string trimmedField = field.Trim();
                            string value = (string)record[field];

                            // If the post doesn't exist, we need to import various stuff from the page.
                            if (useDatabase)
                            {
                                if (trimmedField == "#_Post_Likes")
                                {
                                    // Yuck: whole number likes can have decimal points in the data.
                                    // Yuck: some rows are empty, or have invalid entries.
                                    if (!int.TryParse(value, NumberStyles.AllowDecimalPoint, null, out int numberOfLikes))
                                    {
                                        Console.WriteLine("Cannot parse number of likes. Skipping...");
                                        post.Reactions = new Reactions
                                        {
                                            Summary = new ReactionsSummary { TotalCount = -1 }
                                        };
                                        continue;
                                    }

                                    post.Reactions = new Reactions
                                    {
                                        Summary = new ReactionsSummary { TotalCount = numberOfLikes }
                                    };
                                }
                                else if (trimmedField == "#_Post_Comments")
                                {
                                    // Yuck: whole number likes can have decimal points in the data.
                                    // Yuck: some rows are empty, or have invalid entries.
                                    if (!int.TryParse(value, NumberStyles.AllowDecimalPoint, null, out int numberOfComments))
                                    {
                                        Console.WriteLine("Cannot parse number of comments. Skipping...");
                                        post.Comments = new Comments
                                        {
                                            Summary = new CommentsSummary { TotalCount = -1 }
                                        };
                                        continue;
                                    }

                                    post.Comments = new Comments
                                    {
                                        Summary = new CommentsSummary { TotalCount = numberOfComments }
                                    };
                                }
                                else if (trimmedField == "#_Post_Shares")
                                {
                                    // Yuck: whole number likes can have decimal points in the data.
                                    // Yuck: some rows are empty, or have invalid entries.
                                    if (!int.TryParse(value, NumberStyles.AllowDecimalPoint, null, out int numberOfShares))
                                    {
                                        Console.WriteLine("Cannot parse number of shares. Skipping...");
                                        post.Shares = new Shares { Count = -1 };
                                        continue;
                                    }

                                    post.Shares = new Shares { Count = numberOfShares };
                                }
                                else if (trimmedField == "Post_Date" || trimmedField == "Excerpt Date")
                                {
                                    DateTime date = DateTime.ParseExact(value, "M/d/yyyy", null);
                                    post.CreatedTime = date;
                                }
                                else if (trimmedField == "Excerpt Copy")
                                {
                                    post.Message = value;
                                }
                            }

                            // Turn the comma separated list of topics into an array.
                            if (trimmedField == "Codes Applied Combined")
                            {
                                IEnumerable<string> topics = value.Split(',').Select(c => c.Trim());
                                post.Topics = topics;
                            }

                            // Get the page from the post.
                            if (trimmedField == "Page Name")
                            {
                                normalizedPageName = Mappings[value.Trim()].DisplayName;
                            }
                        }

                        // Get the nearest data we have for page likes at the time the post was created.
                        Debug.Assert(normalizedPageName != null);
                        post.Page = new ScrapedPage { DisplayName = normalizedPageName };
                        PostScraper.UpdateMetadata(post);

                        // Print the progress to make sure we know something is happening.
                        numberSaved++;
                        Console.WriteLine(numberSaved);

                        // Save the post.
                        yield return PostScraper.Save(post, Refresh.False);
                    }
                }
            }
        }
    }
}
