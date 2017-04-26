using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FacebookCivicInsights.Data.Translator
{
    public class GoogleTranslator
    {
        public GoogleTranslatorResult Translate(string sourceLanguage, string targetLanguage, string query)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "https://translate.googleapis.com/translate_a/single?client=gtx&dt=t";
                request += $"&sl={sourceLanguage}";
                request += $"&tl={targetLanguage}";
                request += $"&q={WebUtility.HtmlEncode(query)}";

                try
                {
                    string result = client.GetStringAsync(request).Result;
                    
                    // Data is in the form:
                    // [
                    //   [
                    //     [
                    //       TRANSLATION,
                    //       ...
                    //     ],
                    //     ...
                    //   ],
                    //   ....
                    // ]
                    JArray root = JsonConvert.DeserializeObject<JArray>(result);
                    JToken translationRoot = root[0];
                    JToken translation = translationRoot[0];
                    string translationString = translation[0].Value<string>();

                    return new GoogleTranslatorResult
                    {
                        SourceLanguage = sourceLanguage,
                        TargetLanguage = targetLanguage,
                        Query = query,
                        Result = translationString
                    };
                }
                catch (AggregateException)
                {
                    return null;
                }
            }
        }
    }
}
