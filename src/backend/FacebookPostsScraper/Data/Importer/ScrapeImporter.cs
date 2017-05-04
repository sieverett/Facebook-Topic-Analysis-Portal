using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Elasticsearch.Net;
using FacebookCivicInsights.Models;
using FacebookCivicInsights.Data.Scraper;
using Facebook.Models;

namespace FacebookCivicInsights.Data.Importer
{
    public class ScrapeImporter
    {
        private PageScraper PageScraper { get; }
        private ElasticSearchRepository<PageMetadata> PageMetadataRepository { get; }
        private PostScraper PostScraper { get; }

        public ScrapeImporter(PageScraper pageScraper, ElasticSearchRepository<PageMetadata> pageMetadataRepository, PostScraper postScraper)
        {
            PageScraper = pageScraper;
            PageMetadataRepository = pageMetadataRepository;
            PostScraper = postScraper;
        }

        // Yuck: each page has a number of different names in the data.
        private Dictionary<string, PageMetadata> _mappings;
        private Dictionary<string, PageMetadata> Mappings
        {
            get
            {
                if (_mappings == null)
                {
                    _mappings = new Dictionary<string, PageMetadata>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "hun sen", PageMetadataRepository.Get("Hun Sen") },

                        { "sam rainsy", PageMetadataRepository.Get("Sam Rainsy") },
                        { "samrainsy",  PageMetadataRepository.Get("Sam Rainsy") },

                        { "cambodian people's party", PageMetadataRepository.Get("Cambodian People's Party") },
                        { "ccp",                      PageMetadataRepository.Get("Cambodian People's Party") },
                        { "cpp",                      PageMetadataRepository.Get("Cambodian People's Party") },

                        { "cambodia national rescue party", PageMetadataRepository.Get("Cambodia National Resuce Party") },
                        { "cnrp",                           PageMetadataRepository.Get("Cambodia National Resuce Party") },

                        { "voice of america khmer", PageMetadataRepository.Get("Voice of America Khmer") },
                        { "voakhmer",               PageMetadataRepository.Get("Voice of America Khmer") },
                        { "voa khmer",              PageMetadataRepository.Get("Voice of America Khmer") },

                        { "post khmer", PageMetadataRepository.Get("Post Khmer") },

                        { "radio free asia khmer", PageMetadataRepository.Get("Radio Free Asia Khmer") },
                        { "rfa",                   PageMetadataRepository.Get("Radio Free Asia Khmer") },
                        { "rfa khmer",             PageMetadataRepository.Get("Radio Free Asia Khmer") },
                        { "rfa cambodia",          PageMetadataRepository.Get("Radio Free Asia Khmer") },

                        { "radio france", PageMetadataRepository.Get("Radio France") },
                        { "rfi khmer",    PageMetadataRepository.Get("Radio France") },
                        { "rfi",          PageMetadataRepository.Get("Radio France") },

                        { "vodhotnews", PageMetadataRepository.Get("VoDHotNews") },
                        { "vod",        PageMetadataRepository.Get("VoDHotNews") },
                        { "vod khmer",  PageMetadataRepository.Get("VoDHotNews") },

                        { "camnews", PageMetadataRepository.Get("Camnews") },

                        { "moeys",                                    PageMetadataRepository.Get("Moey") },
                        { "moey",                                     PageMetadataRepository.Get("Moey") },
                        { "ministry of education, youth and spots",   PageMetadataRepository.Get("Moey") },
                        { "Ministry of Education, Youth, and Sports", PageMetadataRepository.Get("Moey") },

                        { "anti-corruption unit", PageMetadataRepository.Get("Anti-corruption Unit") },
                        { "acu",                  PageMetadataRepository.Get("Anti-corruption Unit") },

                        { "kem sokha", PageMetadataRepository.Get("Kem Sokha") },

                        { "hun many", PageMetadataRepository.Get("Hun Many") },

                        { "ministry of commerce", PageMetadataRepository.Get("Ministry of Commerce") },
                        { "mocommerce",           PageMetadataRepository.Get("Ministry of Commerce") },

                        { "cambodia express news",  PageMetadataRepository.Get("Cambodia Express News") },
                        { "cambodian express news", PageMetadataRepository.Get("Cambodia Express News") }
                    };
                }

                return _mappings;
            }
        }

        private static void Read(IEnumerable<string> csvs, Action<IDictionary<string, object>> recordAction)
        {
            foreach (string csv in csvs)
            {
                // Fire up the CSV reader.
                using (var fileStream = new FileStream(csv, FileMode.Open))
                using (var streamReader = new StreamReader(fileStream))
                using (var csvReader = new CsvReader(streamReader))
                {
                    var records = csvReader.GetRecords<dynamic>();
                    foreach (IDictionary<string, object> record in records)
                    {
                        recordAction(record);
                    }
                }
            }
        }

        public IEnumerable<ScrapedPage> ImportPages(IEnumerable<string> fanCountCSVs)
        {
            var pages = new List<ScrapedPage>();
            DateTime now = DateTime.Now;
            int numberSaved = 0;

            Read(fanCountCSVs, record =>
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
                    PageMetadata mappedPage = Mappings[pageName.Trim()];

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
                    ScrapedPage savedPage = PageScraper.Closest(p => p.Name, mappedPage.Name, date);
                    if (savedPage == null || savedPage.Date != date)
                    {
                        // Page doesn't have this date already. Add it.
                        savedPage = new ScrapedPage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = mappedPage.Name,
                            Category = mappedPage.Category,
                            FacebookId = mappedPage.FacebookId,
                            Date = date,
                            FanCount = numberOfLikes
                        };
                    }
                    else
                    {
                        // Page already has this date already. Update it.
                        savedPage.FanCount = numberOfLikes;
                    }

                    // Save the page.
                    numberSaved++;
                    Console.WriteLine(numberSaved);
                    pages.Add(PageScraper.Save(savedPage, Refresh.False));
                }
            });

            return pages;
        }

        public IEnumerable<ScrapedPost> ImportPosts(IEnumerable<string> postCSVs)
        {
            var posts = new List<ScrapedPost>();
            DateTime now = DateTime.Now;
            int numberSaved = 0;

            Read(postCSVs, record =>
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
                        normalizedPageName = Mappings[value.Trim()].Name;
                    }
                }

                // Get the nearest data we have for page likes at the time the post was created.
                Debug.Assert(normalizedPageName != null);
                PostScraper.UpdateMetadata(post, normalizedPageName);

                // Print the progress to make sure we know something is happening.
                numberSaved++;
                Console.WriteLine(numberSaved);

                // Save the post.
                posts.Add(PostScraper.Save(post, Refresh.False));
            });

            return posts;
        }
    }
}
