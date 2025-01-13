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
        Uri guessEndpoint;
        Uri wordEndpoint;

        HttpClient client = new HttpClient();

        public MoonAPIReader()
        {
            apiPath = new Uri("https://wordle-game-api1.p.rapidapi.com", UriKind.Absolute);
            guessEndpoint = new Uri(apiPath, "/guess");
            wordEndpoint = new Uri(apiPath, "/word");
        }

        public async Task<WordResult> getWordOfTheDayJSON()
        {
            WordResult wordResult = new WordResult();

            // CancellationTokenSource cts = new CancellationTokenSource();
            // cts.CancelAfter(TimeSpan.FromSeconds(5));
            // HttpResponseMessage response = await client.GetAsync(wordEndpoint, HttpCompletionOption.ResponseContentRead, cts.Token);

            HttpResponseMessage resp = await client.GetAsync(wordEndpoint, HttpCompletionOption.ResponseHeadersRead);
            var jsonResponse = await resp.Content.ReadAsStringAsync();
            JsonDocument jsonDoc = JsonDocument.Parse(jsonResponse);

            JsonElement jsonRoot = jsonDoc.RootElement;

            string word = jsonRoot.GetProperty("word").GetString();
            bool isOk = jsonRoot.GetProperty("isOk").GetBoolean();
            string error = jsonRoot.GetProperty("error").GetString();

            wordResult.Word = word;
            wordResult.IsOk = isOk;
            wordResult.Error = error;

            return wordResult;
        }

        public async Task<WordResult> getRandomWordJSON()
        {
            WordResult wordResult = new WordResult();

            HttpResponseMessage response = await client.GetAsync(wordEndpoint, HttpCompletionOption.ResponseHeadersRead);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            JsonDocument jsonDoc = JsonDocument.Parse(jsonResponse);

            JsonElement jsonRoot = jsonDoc.RootElement;

            string word = jsonRoot.GetProperty("word").GetString();
            bool isOk = jsonRoot.GetProperty("isOk").GetBoolean();
            string error = jsonRoot.GetProperty("error").GetString();

            wordResult.Word = word;
            wordResult.IsOk = isOk;
            wordResult.Error = error;


            return wordResult;
        }

        public async void testCheckGuess()
        {
            string content = "{\"word\":\"apple\",\"timezone\":\"UTC + 0\"}";

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://wordle-game-api1.p.rapidapi.com/guess"),
                Headers =
                {
                    { "x-rapidapi-key", "10c1f99ba6msh404cc93f96cd25bp1974b1jsnc7de848f1361" },
                    { "x-rapidapi-host", "wordle-game-api1.p.rapidapi.com" },
                },
                Content = new StringContent(content) 
                { 
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine(body);
            }
        }

        public async void testGetWoTD()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://wordle-game-api1.p.rapidapi.com/word"),
                Headers =
                {
                    { "x-rapidapi-key", "10c1f99ba6msh404cc93f96cd25bp1974b1jsnc7de848f1361" },
                    { "x-rapidapi-host", "wordle-game-api1.p.rapidapi.com" },
                }
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine(body);
            }
        }


    }
}
