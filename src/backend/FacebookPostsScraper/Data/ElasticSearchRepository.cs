using Elasticsearch.Net;
using Facebook;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FacebookCivicInsights.Data
{
    public class ElasticSearchRepository<T> where T: class, new()
    {
        private ElasticClient Client { get; }

        public ElasticSearchRepository(string url, string defaultIndex)
        {
            var node = new Uri(url);
            var settings = new ConnectionSettings(node).DefaultIndex(defaultIndex);
            Client = new ElasticClient(settings);
        }

        public T Save(T data)
        {
            Client.Index(new IndexRequest<T>(data)
            {
                Refresh = Refresh.WaitFor
            });

            return data;
        }

        public T Get(string id) => Client.Get<T>(id).Source;

        public long TotalCount() => Client.Count<T>().Count;

        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 50;
        private const int MaxPageSize = 10000;

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
        public static PagedResponse<T> All<T>(this ElasticSearchRepository<T> repository,
                                              int pageNumber, int pageSize,
                                              Expression<Func<T, object>> orderPath, OrderingType? order,
                                              Expression<Func<T, object>> searchPath, DateTime? since, DateTime? until) where T : class, new()
        {
            var paging = new PagedResponse { PageNumber = pageNumber, PageSize = pageSize };
            var ordering = new Ordering<T>
            {
                Order = order ?? OrderingType.Descending,
                Path = orderPath
            };

            Func<QueryContainerDescriptor<T>, QueryContainer> search = query =>
            {
                return query.DateRange(d =>
                {
                    return d.Field(searchPath).GreaterThanOrEquals(since ?? DateTime.MinValue)
                                              .LessThanOrEquals(until ?? DateTime.MaxValue);
                });
            };

            return repository.Paged(paging, ordering, search);
        }
    }
}
