using System;
using System.Linq.Expressions;
using Elasticsearch.Net;
using Facebook;
using Nest;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace FacebookCivicInsights.Data
{
    public class ElasticSearchRepository<T> where T: class, new()
    {
        private string DefaultIndex { get; }
        public ElasticClient Client { get; }

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
            if (!response.IsValid)
            {
                throw new InvalidOperationException(response.ServerError.ToString());
            }
            return data;
        }

        public T Get(string id) => Client.Get<T>(id).Source;

        public const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 50;
        public const int MaxPageSize = 10000;

        public AllResponse<T> All(object query = null, IList<SortField> sort = null)
        {
            ElasticSearchPagedResponse<T> response = Paged(0, int.MaxValue, query, sort);
            return new AllResponse<T>
            {
                Data = response.AllData(),
                TotalCount = response.TotalCount
            };
        }

        public ElasticSearchPagedResponse<T> Paged(int pageNumber, int pageSize, object query = null, IList<SortField> sort = null)
        {
            QueryContainer queryContainer = query as QueryContainer;
            if (queryContainer == null && query != null)
            {
                string stringRepresentation = JsonConvert.SerializeObject(query);
                queryContainer = new QueryContainer(new RawQuery(stringRepresentation));/*
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(stringRepresentation)))
                {
                    queryContainer = Client.Serializer.Deserialize<QueryContainer>(stream);
                }*/
            }

            return Paged(pageNumber, pageSize, queryContainer, sort);
        }

        public ElasticSearchPagedResponse<T> Paged(int pageNumber, int pageSize, QueryContainer query, IList<SortField> sort)
        {
            // If the page number was invalid, use the default page number.
            pageNumber = Math.Max(DefaultPageNumber, pageNumber);

            // If the page size was invalid, use the default page size.
            pageSize = Math.Max(DefaultPageSize, pageSize);
            pageSize = Math.Min(pageSize, MaxPageSize);

            var content = new ElasticSearchPagedResponse<T>
            {
                Repository = this,
                Query = query,
                Sort = sort,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Page numbers start at 0 for Elasticsearch.
            int from = (content.PageNumber - 1) * pageSize;
            ISearchResponse<T> searchResponse = Client.Search<T>(new SearchRequest<T>
            {
                Query = query,
                ///Sort = sort?.Cast<ISort>()?.ToList(),
                From = from,
                Size = pageSize
            });
            content.Data = searchResponse.Documents.ToArray();
            content.TotalCount = searchResponse.Total;

            // If the page number is not in range, use the default search.
            if (content.StartItemIndex > content.TotalCount)
            {
                return Paged(-1, -1, query, sort);
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

            IDeleteResponse response = Client.Delete<T>(id);
            if (!response.IsValid)
            {
                throw new InvalidOperationException(response.ServerError.ToString());
            }
            return deletedDocument;
        }
    }
}
