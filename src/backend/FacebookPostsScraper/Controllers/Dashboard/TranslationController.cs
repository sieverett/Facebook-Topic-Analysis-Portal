using FacebookCivicInsights.Data.Translator;
using Microsoft.AspNetCore.Mvc;

namespace FacebookCivicInsights.Controllers.Dashboard
{
    [Route("/api/dashboard/translate")]
    public class TranslationController
    {
        [HttpGet("{message}")]
        public GoogleTranslatorResult Translate(string message)
        {
            return new GoogleTranslator().Translate("km", "en", message);
        }
    }
}
