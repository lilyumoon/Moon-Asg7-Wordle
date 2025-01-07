using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

internal class ApiReader
    {

        // Simple Class to store the DayNumber and the Word, and RequestType
        internal class WordDay
        {
            public int DayNumber { get; set; }
            public string Word { get; set; }
            public string RequestType { get; set; }
            public WordDay() { }
    }


    // Method to Call the API  asynchronously.
    public async Task<String> getWordOfTheDay()
    {
        string word = string.Empty; 
        HttpClient httpClient = new HttpClient();
        Uri uri = new Uri("http://18.191.113.18/phpapi/Api.php?type=day");
        // Example response:  Word=SNAKE

        HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
        var responseData = await response.Content.ReadAsStringAsync();

        word = responseData.Replace("Word=", "");

        word = word.Replace("\n", ""); // Remove trailing newline char
        return word;
    }

  
    public async Task<String> getRandomWord()
    {
        string word = string.Empty;
        HttpClient httpClient = new HttpClient();
        Uri uri = new Uri("http://18.191.113.18/phpapi/Api.php?type=random");
        // Example response:  Word=RADIX

        HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
        var responseData = await response.Content.ReadAsStringAsync();

        word = responseData.Replace("Word=", "");
        word = word.Replace("\n", ""); // Remove trailing newline char
        return word;

    }
   
    public async Task<String> getWordForSpecificDay(int dayNumber)
    {
        
        string word = string.Empty;
        HttpClient httpClient = new HttpClient();
        Uri uri = new Uri("http://18.191.113.18/phpapi/Api.php?type=specific&dayNumber=" + dayNumber.ToString());

        // Example Response:  Word=THEIR

        HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
        var responseData = await response.Content.ReadAsStringAsync();

        // Parse the response
        word = responseData.Replace("Word=", "");
        word = word.Replace("\n", "");  // Remove trailing newline char
        return word;
    }
  



    /* ********************************************************************************** */

    // Methods using JSON  JavaScript Object Notation
    // Responses in JSON allow for more self describing complex data
 

    public async Task<WordDay> getWordOfTheDayJSON()
    {
        WordDay wordDay = new WordDay();
        // string word = "";
        HttpClient httpClient = new HttpClient();
        Uri uri = new Uri("http://18.191.113.18/phpapi/ApiJSON.php?type=day");
        // Example response:  {"word": "SNAKE","dayNumber": -1,"type": "day"}

        HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        JsonDocument jsonDoc = JsonDocument.Parse(jsonResponse);

        // Access the root element
        JsonElement jsonRoot = jsonDoc.RootElement;

        // Accessing properties
        string word = jsonRoot.GetProperty("word").GetString();
        int dayNumber = jsonRoot.GetProperty("dayNumber").GetInt32();
        string type = jsonRoot.GetProperty("type").GetString();

        wordDay.Word = word;
        wordDay.DayNumber = dayNumber;
        wordDay.RequestType = type;

        return wordDay;
    }

    public async Task<WordDay> getRandomWordJSON()
    {
        WordDay wordDay = new WordDay();
        HttpClient httpClient = new HttpClient();
        Uri uri = new Uri("http://18.191.113.18/phpapi/ApiJSON.php?type=random");
        // Example response:  {"word": "PRONE","dayNumber": -1,"type": "random"}

        HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        JsonDocument jsonDoc = JsonDocument.Parse(jsonResponse);

        // Access the root element
        JsonElement jsonRoot = jsonDoc.RootElement;


        // Accessing properties
        string word = jsonRoot.GetProperty("word").GetString();
        int dayNumber = jsonRoot.GetProperty("dayNumber").GetInt32();
        string type = jsonRoot.GetProperty("type").GetString();

        wordDay.Word = word;
        wordDay.DayNumber = dayNumber;
        wordDay.RequestType = type;

        return wordDay;
    }
    public async Task<WordDay> getWordForSpecificDayJSON(int dayNumber)
    {
        WordDay wordDay = new WordDay();
        HttpClient httpClient = new HttpClient();
        Uri uri = new Uri("http://18.191.113.18/phpapi/ApiJSON.php?type=specific&dayNumber=" + dayNumber.ToString());
        // Example response:  {"word": "THEIR","dayNumber": 3,"type": "specific"}

        HttpResponseMessage response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        JsonDocument jsonDoc = JsonDocument.Parse(jsonResponse);

        // Access the root element
        JsonElement jsonRoot = jsonDoc.RootElement;


        // Accessing properties
        string word = jsonRoot.GetProperty("word").GetString();
        int dayNum = jsonRoot.GetProperty("dayNumber").GetInt32();
        string type = jsonRoot.GetProperty("type").GetString();

        wordDay.Word = word;
        wordDay.DayNumber = dayNum;
        wordDay.RequestType = type;

        return wordDay;
    }
}
