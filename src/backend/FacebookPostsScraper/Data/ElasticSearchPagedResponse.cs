using System;
using Facebook;
using Nest;

namespace FacebookCivicInsights.Data
{
    public class ElasticSearchPagedResponse<T> : PagedResponse<T> where T: class, new()
    {
        public long TotalCount { get; set; }
        public long NumberOfPages => (TotalCount - 1) / PageSize + 1;

        internal ElasticSearchRepository<T> Repository { get; set; }
        internal Func<QueryContainerDescriptor<T>, QueryContainer> Search { get; set; }
        public Ordering<T> Ordering { get; set; }

        public override PagedResponse<T> PreviousPage()
        {
            // Nothing before.
            if (PageNumber == 1)
            {
                return null;
            }

            // Ask the repository for the previous page.
            return Repository.Paged(new PagedResponse
            {
                PageNumber = PageNumber - 1,
                PageSize = PageSize
            }, Ordering, Search);
        }

        public override PagedResponse<T> NextPage()
        {
            // Nothing after.
            if (PageNumber == NumberOfPages)
            {
                return null;
            }

            // Ask the repository for the next page.
            return Repository.Paged(new PagedResponse
            {
                PageNumber = PageNumber + 1,
                PageSize = PageSize
            }, Ordering, Search);
        }
    }
}
