using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ChatTextTranslation
{
    public static class TextTranslator
    {
        [FunctionName("TextTranslator")]
        [return: EventHub(Constants.EventHubReceiver, Connection = "EVENTHUB_CONNECTION_STRING")]
        public static string Run([EventHubTrigger(Constants.EventHubSender, Connection = "EVENTHUB_CONNECTION_STRING")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                    Output output = new Output();

                    log.LogInformation($"Translate string: {messageBody}");
                    
                    // Get translation into all supported languages
                    string translatedString = TranslateString(messageBody);

                    if (translatedString != null)
                    {
                        log.LogInformation($"Translated string: {translatedString}");
                        output.TranslatedString = translatedString;
                        output.OriginalString = messageBody;

                        var outputJson = JsonConvert.SerializeObject(output);

                        return outputJson;
                    }
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();

            return null;
        }

        [FunctionName("TranslateString")]
        public static string TranslateString(string input)
        {
            // Full list of languages documented here - translation property: https://api.cognitive.microsofttranslator.com/languages?api-version=3.0
            string host = Constants.AzureBaseURLWithRegion;
            string route = Constants.TranslatedLanguagesRoute;

            // Your Text Translator subscription key.
            string CSSubscriptionKey = Environment.GetEnvironmentVariable("TRANSLATORTEXT_KEY");

            System.Object[] body = new System.Object[] { new { Text = input } };
            var jsonBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Set the method to POST
                request.Method = HttpMethod.Post;

                // Construct the full URI
                request.RequestUri = new Uri(host + route);

                // Add the serialized JSON object to your request
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                // Add the authorization header
                request.Headers.Add("Ocp-Apim-Subscription-Key", CSSubscriptionKey);

                // Send request, get response
                var response = client.SendAsync(request).Result;
                string jsonResponse = response.Content.ReadAsStringAsync().Result;

                return jsonResponse;
            }
        }

        class Output
        {
            public string TranslatedString { get; set; }

            public string OriginalString { get; set; }
        }
    }
}
