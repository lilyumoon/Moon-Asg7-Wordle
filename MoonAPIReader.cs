using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace Moon_Asg7_Wordle
{
    public class MoonAPIReader
    {

        public class WordResult
        {
            public string Word { get; set; }
            public bool IsOk { get; set; }
            public string Error {  get; set; }

            public WordResult() { }
        }

        Uri apiPath;
        Uri currentWordEndpoint;
        Uri yesterdayWordEndpoint;
        Uri tomorrowWordEndpoint;
        Uri choiceWordEndpoint;
        Uri apiHealthEndpoint;

        HttpClient client = new HttpClient();

        public MoonAPIReader()
        {
            apiPath = new Uri("https://wordle-api3.p.rapidapi.com", UriKind.Absolute);
            currentWordEndpoint = new Uri(apiPath, "getcurrentword");
            yesterdayWordEndpoint = new Uri(apiPath, "getwordyesterday");
            tomorrowWordEndpoint = new Uri(apiPath, "getwordtomorrow");
            choiceWordEndpoint = new Uri(apiPath, "getwordfor");
            apiHealthEndpoint = new Uri(apiPath, "health");
        }

        /// <summary>
        /// Gets the health of the API in the format of {"status":"ok"}
        /// </summary>
        public async Task<bool> isApiHealthy()
        {
            bool isHealthy = true;

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = apiHealthEndpoint,
                Headers =
                {
                    { "x-rapidapi-key", "10c1f99ba6msh404cc93f96cd25bp1974b1jsnc7de848f1361" },
                    { "x-rapidapi-host", "wordle-api3.p.rapidapi.com" },
                },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                JsonDocument jsonDoc = JsonDocument.Parse(body);
                JsonElement jsonRoot = jsonDoc.RootElement;

                string status = jsonRoot.GetProperty("status").GetString();

                Console.WriteLine(status);

                if (!string.Equals(status, "ok"))
                    isHealthy = false;
            }

            return isHealthy;
        }

        /// <summary>
        /// Gets Today's word in the format of {"date":"[date]","word":"[word]"}
        /// </summary>
        public async Task<string> getWordForToday()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = currentWordEndpoint,
                Headers =
                {
                    { "x-rapidapi-key", "10c1f99ba6msh404cc93f96cd25bp1974b1jsnc7de848f1361" },
                    { "x-rapidapi-host", "wordle-api3.p.rapidapi.com" },
                }
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine(body);

                JsonDocument jsonDoc = JsonDocument.Parse(body);
                JsonElement jsonRoot = jsonDoc.RootElement;

                string word = jsonRoot.GetProperty("word").GetString();

                Console.WriteLine(word);
                return word;
            }
        }

        /// <summary>
        /// Gets yesterday's word in the format of {"date":"[date]","word":"[word]"}
        /// </summary>
        public async Task<string> getWordForYesterday()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = yesterdayWordEndpoint,
                Headers =
                {
                    { "x-rapidapi-key", "10c1f99ba6msh404cc93f96cd25bp1974b1jsnc7de848f1361" },
                    { "x-rapidapi-host", "wordle-api3.p.rapidapi.com" },
                }
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                JsonDocument jsonDoc = JsonDocument.Parse(body);
                JsonElement jsonRoot = jsonDoc.RootElement;

                string word = jsonRoot.GetProperty("word").GetString();

                Console.WriteLine(word);
                return word;
            }
        }

        /// <summary>
        /// Gets (predicts) tomorrow's word in the format of {"date":"[date]","word":"[word]"}
        /// </summary>
        public async Task<string> getWordForTomorrow()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = tomorrowWordEndpoint,
                Headers =
                {
                    { "x-rapidapi-key", "10c1f99ba6msh404cc93f96cd25bp1974b1jsnc7de848f1361" },
                    { "x-rapidapi-host", "wordle-api3.p.rapidapi.com" },
                },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                JsonDocument jsonDoc = JsonDocument.Parse(body);
                JsonElement jsonRoot = jsonDoc.RootElement;

                string word = jsonRoot.GetProperty("word").GetString();

                Console.WriteLine(word);
                return word;
            }
        }

        /// <summary>
        /// Gets word for a specified date in the format of {"date":"[date]","word":"[word]"}
        /// </summary>
        /// <param name="date"></param>
        public async Task<string> getWordForDate(DateTime date)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                // Building an explicit Uri here because of persistent issues with base/relative path concatenation in Uri constructor...
                RequestUri = new Uri($"https://wordle-api3.p.rapidapi.com/getwordfor/{date.ToString("yyyy-MM-dd")}"),
                Headers =
                {
                    { "x-rapidapi-key", "10c1f99ba6msh404cc93f96cd25bp1974b1jsnc7de848f1361" },
                    { "x-rapidapi-host", "wordle-api3.p.rapidapi.com" },
                },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                JsonDocument jsonDoc = JsonDocument.Parse(body);
                JsonElement jsonRoot = jsonDoc.RootElement;

                string word = jsonRoot.GetProperty("word").GetString();

                Console.WriteLine(word);
                return word;
            }
        }

        /// <summary>
        /// Gets word for a random date in the format of {"date":"[date]","word":"[word]"}
        /// </summary>
        public async Task<string> getWordForRandomDate()
        {
            // pick a random date
            DateTime startDate = new DateTime(2024, 01, 01);
            DateTime endDate = DateTime.Today;
            DateTime randomDate = getRandomDate(startDate, endDate);

            // call getWordForDate passing random date in as parameter
            string word = await getWordForDate(randomDate);
            return word;
        }

        /// <summary>
        /// Gets a random date between a start date and an end date.
        /// </summary>
        /// <param name="startDate">The beginning of the date range.</param>
        /// <param name="endDate">The end of the date range.</param>
        /// <returns>A random date within the calculated range.</returns>
        private DateTime getRandomDate(DateTime startDate, DateTime endDate)
        {
            Random random = new Random();

            // get the range of days
            int range = (endDate - startDate).Days;

            // get a random number of days to add to the start date
            int randomDays = random.Next(range + 1);

            return startDate.AddDays(randomDays);
        }
    }
}
