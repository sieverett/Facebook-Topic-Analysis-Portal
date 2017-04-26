namespace FacebookCivicInsights.Data
{
    public class Ordering<T>
    {
        public Ordering(string path, OrderingType? order)
        {
            Path = path;
            Order = order ?? OrderingType.Descending;
        }

        public string Path { get; }
        public OrderingType Order { get; }
    }
}
