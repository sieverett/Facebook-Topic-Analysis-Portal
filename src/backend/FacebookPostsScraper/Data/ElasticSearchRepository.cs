using Elasticsearch.Net;
using Facebook;
using FacebookPostsScraper.Data;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FacebookCivicInsights.Data
{
    public class ElasticSearchRepository<T> where T: class, new()
    {
        private string DefaultIndex { get; }
        private ElasticClient Client { get; }

        public ElasticSearchRepository(string url, string defaultIndex, Func<CreateIndexDescriptor, ICreateIndexRequest> createIndex = null)
        {
            var node = new Uri(url);
            var settings = new ConnectionSettings(node).DefaultIndex(defaultIndex);
            DefaultIndex = defaultIndex;
            Client = new ElasticClient(settings);

            if (createIndex != null)
            {
                Client.CreateIndex(defaultIndex, createIndex);
            }
        }

        public T Save(T data, Refresh refresh = Refresh.WaitFor)
        {
            Client.Index(data, idx => idx.Index(DefaultIndex).Refresh(refresh));
            return data;
        }

        public T Get(string id) => Client.Get<T>(id).Source;

        public long TotalCount() => Client.Count<T>().Count;

        public const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 50;
        public const int MaxPageSize = 10000;

        public PagedResponse<T> Paged(PagedResponse paging = null, Ordering<T> ordering = null, Func<QueryContainerDescriptor<T>, QueryContainer> search = null)
        {
            // If the page number was invalid, use the default page number.
            int pageNumber = DefaultPageNumber;
            if (paging?.PageNumber >= DefaultPageNumber)
            {
                pageNumber = paging.PageNumber;
            }

            // If the page size was invalid, use the default page size.
            int pageSize = DefaultPageSize;
            if (paging != null && (paging.PageSize > 0 && paging.PageSize <= MaxPageSize))
            {
                pageSize = paging.PageSize;
            }

            var content = new PagedContent<T>
            {
                Repository = this,
                Ordering = ordering,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = TotalCount()
            };

            // If the page number is not in range, use the default page number.
            if (content.StartItemIndex >= content.TotalCount)
            {
                content.PageNumber = DefaultPageNumber;
            }

            // Page numbers start at 0 for Elasticsearch.
            int from = (content.PageNumber - 1) * pageSize;
            IEnumerable<T> all = Client.Search<T>(s =>
            {
                s = s.From(from).Size(pageSize);

                // The user can specify an ordering of the data returned.
                if (ordering != null)
                {
                    if (ordering.Order == OrderingType.Ascending)
                    {
                        s = s.Sort(sort => sort.Ascending(ordering.Path));
                    }
                    else
                    {
                        s = s.Sort(sort => sort.Descending(ordering.Path));
                    }
                }

                // The user can specify a search query to filter data.
                if (search != null)
                {
                    s = s.Query(search);
                }

                return s;
            }).Documents;

            content.Data = all;
            return content;
        }

        public T Delete(string id)
        {
            T deletedDocument = Get(id);
            if (deletedDocument == null)
            {
                throw new InvalidOperationException("No such object");
            }

            Client.Delete<T>(id);
            return deletedDocument;
        }
    }

    public static class ElasticSearchRepositoryExtensions
    {
        private static Ordering<T> GetOrdering<T>(Expression<Func<T, object>> orderPath, OrderingType? order)
        {
            return new Ordering<T>
            {
                Order = order ?? OrderingType.Descending,
                Path = orderPath
            };
        }

        private static Func<QueryContainerDescriptor<T>, QueryContainer> GetSearch<T>(Expression<Func<T, object>> searchPath, DateTime? since, DateTime? until) where T : class, new()
        {
            return query =>
            {
                return query.DateRange(d =>
                {
                    return d.Field(searchPath).GreaterThanOrEquals(since ?? DateTime.MinValue)
                                              .LessThanOrEquals(until ?? DateTime.MaxValue);
                });
            };
        }

        public static PagedResponse<T> All<T>(this ElasticSearchRepository<T> repository,
            int pageNumber, int pageSize,
            Expression<Func<T, object>> orderPath, OrderingType? order,
            Expression<Func<T, object>> searchPath, DateTime? since, DateTime? until) where T : class, new()
        {
            var paging = new PagedResponse { PageNumber = pageNumber, PageSize = pageSize };
            Ordering<T> ordering = GetOrdering(orderPath, order);
            Func<QueryContainerDescriptor<T>, QueryContainer> search = GetSearch(searchPath, since, until);

            return repository.Paged(paging, ordering, search);
        }

        public static byte[] Export<T>(this ElasticSearchRepository<T> repository,
            Expression<Func<T, object>> orderPath, OrderingType? order,
            Expression<Func<T, object>> searchPath, DateTime? since, DateTime? until,
            Func<T, dynamic> mapping) where T : class, new()
        {
            var paging = new PagedResponse
            {
                PageNumber = ElasticSearchRepository<T>.DefaultPageNumber,
                PageSize = ElasticSearchRepository<T>.MaxPageSize
            };
            Ordering<T> ordering = GetOrdering(orderPath, order);
            Func<QueryContainerDescriptor<T>, QueryContainer> search = GetSearch(searchPath, since, until);

            PagedResponse<T> response = repository.Paged(paging, ordering, search);
            IEnumerable<T> data = response.AllData().Flatten();

            return CsvSerialization.Serialize(data, mapping);
        }
    }
}
