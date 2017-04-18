using System;
using System.Linq.Expressions;

namespace FacebookCivicInsights.Data
{
    public class Search<T>
    {
        public Expression<Func<T, string>> Field { get; set; }
        public object Value { get; set; }
    }
}
