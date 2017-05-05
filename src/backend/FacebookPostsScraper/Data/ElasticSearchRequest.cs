using System;
using System.Collections.Generic;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FacebookCivicInsights.Data
{
    public class SortJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(IList<SortField>);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var fields = new List<SortField>();

            JArray sortFields = JArray.Load(reader);
            foreach (JObject sortField in sortFields.Children<JObject>())
            {
                foreach (var field in sortField)
                {
                    string ordering = field.Value.Value<string>("order");
                    fields.Add(new SortField
                    {
                        Field = field.Key,
                        Order = ordering != "asc" ? SortOrder.Descending : SortOrder.Ascending
                    });
                }
            }

            return fields;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class ElasticSearchRequest : PagedRequest
    {
        public object Query { get; set; }

        [JsonConverter(typeof(SortJsonConverter))]
        public IList<SortField> Sort { get; set; }
    }
}
