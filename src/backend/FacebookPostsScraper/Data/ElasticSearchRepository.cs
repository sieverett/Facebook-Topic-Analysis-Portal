using System;
using System.Linq.Expressions;
using Elasticsearch.Net;
using Facebook;
using Nest;
using System.Linq;

namespace FacebookCivicInsights.Data
{
    public class ElasticSearchRepository<T> where T: class, new()
    {
        private string DefaultIndex { get; }
        private ElasticClient Client { get; }

        public ElasticSearchRepository(ConnectionSettings settings, string defaultIndex, Func<CreateIndexDescriptor, ICreateIndexRequest> createIndex = null)
        {
            settings = settings.DefaultIndex(defaultIndex);
            DefaultIndex = defaultIndex;
            Client = new ElasticClient(settings);

            if (createIndex != null)
            {
                Client.CreateIndex(defaultIndex, createIndex);
            }
        }

        public T Save(T data, Refresh refresh = Refresh.WaitFor)
        {
            IIndexResponse response = Client.Index(data, idx => idx.Index(DefaultIndex).Refresh(refresh));
            return data;
        }

        public T Get(string id) => Client.Get<T>(id).Source;

        public const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 50;
        public const int MaxPageSize = 10000;

        public ElasticSearchPagedResponse<T> Paged(PagedResponse paging = null, Ordering<T> ordering = null, Func<QueryContainerDescriptor<T>, QueryContainer> search = null)
        {
            return Paged<ElasticSearchPagedResponse<T>>(paging, ordering, search);
        }

        public TPaging Paged<TPaging>(PagedResponse paging = null, Ordering<T> ordering = null, Func<QueryContainerDescriptor<T>, QueryContainer> search = null) where TPaging : ElasticSearchPagedResponse<T>, new()
        {
            // If the page number was invalid, use the default page number.
            int pageNumber = DefaultPageNumber;
            if (paging?.PageNumber >= DefaultPageNumber)
            {
                pageNumber = paging.PageNumber;
            }

            // If the page size was invalid, use the default page size.
            int pageSize = DefaultPageSize;
            if (paging != null && paging.PageSize > 0)
            {
                pageSize = Math.Min(paging.PageSize, MaxPageSize);
            }

            var content = new TPaging
            {
                Repository = this,
                Search = search,
                Ordering = ordering,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Page numbers start at 0 for Elasticsearch.
            int from = (content.PageNumber - 1) * pageSize;
            ISearchResponse<T> searchResponse = Client.Search<T>(s =>
            {
                s = s.From(from).Size(pageSize);

                // The user can specify an ordering of the data returned.
                if (ordering != null)
                {
                    if (ordering.Descending)
                    {
                        s = s.Sort(sort => sort.Descending(ordering.Key));
                    }
                    else
                    {
                        s = s.Sort(sort => sort.Ascending(ordering.Key));
                    }
                }

                // The user can specify a search query to filter data.
                if (search != null)
                {
                    s = s.Query(search);
                }

                return s;
            });

            content.Data = searchResponse.Documents.ToArray();
            content.TotalCount = searchResponse.Total;

            // If the page number is not in range, use the default search.
            if (content.StartItemIndex > content.TotalCount)
            {
                return Paged<TPaging>(null, ordering, search);
            }

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

        public static TResponse All<TResponse, T>(this ElasticSearchRepository<T> repository,
            PagedResponse paging, Ordering<T> ordering,
            Expression<Func<T, object>> searchPath, DateTime? since, DateTime? until) where T : class, new() where TResponse : TimeSearchResponse<T>, new()
        {
            Func<QueryContainerDescriptor<T>, QueryContainer> search = GetSearch(searchPath, since, until);
            TResponse response = repository.Paged<TResponse>(paging, ordering, search);
            response.Ordering = ordering;
            response.Since = since;
            response.Until = until;
            return response;
        }
    }
}
