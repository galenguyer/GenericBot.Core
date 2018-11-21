using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace UrbanDictionary
{
    public class UrbanClientResponse
    {
        [JsonProperty("list")]
        public List<UrbanResult> List = new List<UrbanResult>();

        [JsonConstructor]
        public UrbanClientResponse()
        {
            
        }

        public UrbanClientResponse(string word)
        {
            string responseString;
            using (var client = new WebClient())
            {
                responseString = client.DownloadStringTaskAsync(new Uri($"http://api.urbandictionary.com/v0/define?term={word}")).Result;
            }

            UrbanClientResponse tempResponse = JsonConvert.DeserializeObject<UrbanClientResponse>(responseString);

            List = tempResponse.List;
        }
    }
}