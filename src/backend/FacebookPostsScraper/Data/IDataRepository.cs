using Facebook;

namespace FacebookCivicInsights.Data
{
    public interface IDataRepository<T>
    {
        T Save(T data);
        T Get(string id);

        long TotalCount();

        PagedResponse<T> Paged(PagedResponse paging = null, Ordering<T> ordering = null, Search<T> search = null);
        T Delete(string id);
    }
}
