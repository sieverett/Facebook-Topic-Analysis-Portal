using Facebook;

namespace FacebookCivicInsights.Data
{
    internal class PagedContent<T> : PagedResponse<T> where T: class, new()
    {
        public long TotalCount { get; set; }
        public long NumberOfPages => (TotalCount - 1) / PageSize + 1;

        internal ElasticSearchRepository<T> Repository { get; set; }
        internal Ordering<T> Ordering { get; set; }

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
            }, Ordering);
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
            }, Ordering);
        }
    }
}
