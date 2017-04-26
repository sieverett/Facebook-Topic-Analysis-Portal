namespace FacebookCivicInsights.Data
{
    public class Ordering<T>
    {
        public Ordering(string path, bool? descending)
        {
            Key = path;
            Descending = descending ?? true;
        }

        public string Key { get; }
        public bool Descending { get; }
    }
}
