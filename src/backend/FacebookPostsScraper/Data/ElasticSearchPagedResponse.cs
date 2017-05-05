using System;
using System.Collections.Generic;
using Facebook;
using Nest;
using Newtonsoft.Json;

namespace FacebookCivicInsights.Data
{
    public class ElasticSearchPagedResponse<T> : PagedResponse<T> where T : class, new()
    {
        public long TotalCount { get; set; }
        public long NumberOfPages => (TotalCount - 1) / PageSize + 1;

        internal ElasticSearchRepository<T> Repository { get; set; }

        [JsonIgnore]
        public QueryContainer Query { get; set; }
        public IList<SortField> Sort { get; set; }

        public override PagedResponse<T> PreviousPage()
        {
            // Nothing before.
            if (PageNumber == 1)
            {
                return null;
            }

            // Ask the repository for the previous page.
            return Repository.Paged(PageNumber - 1, PageSize, Query, Sort);
        }

        public override PagedResponse<T> NextPage()
        {
            // Nothing after.
            if (PageNumber == NumberOfPages)
            {
                return null;
            }

            // Ask the repository for the next page.
            return Repository.Paged(PageNumber + 1, PageSize, Query, Sort);
        }

        internal PagedResponse OrderBy(Func<object, object> p)
        {
            throw new NotImplementedException();
        }
    }
}
