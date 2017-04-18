using System;
using System.Linq.Expressions;

namespace FacebookCivicInsights.Data
{
    public class Ordering<T>
    {
        public OrderingType Order { get; set; }
        public Expression<Func<T, object>> Path { get; set; }
    }
}
