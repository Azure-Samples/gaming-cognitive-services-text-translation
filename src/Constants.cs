using System;
using System.Collections.Generic;
using System.Text;

namespace ChatTextTranslation
{
    class Constants
    {
        // Event Hubs
        public const string EventHubSender = "eh-ttchat-sender";
        public const string EventHubReceiver = "eh-ttchat-receiver";

        // Cognitive service
        public const string AzureBaseURLWithRegion = "https://api.cognitive.microsofttranslator.com";
        public const string TranslatedLanguagesRoute = "/translate?api-version=3.0&to=en&to=de&to=it&to=es&to=fr&to=ko&to=ja&to=pt&to=zh-Hans&to=zh-Hant";
    }
}
