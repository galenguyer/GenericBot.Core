using Newtonsoft.Json;
namespace UrbanDictionary
{
    public class UrbanResult
    {
        [JsonProperty("definition")]
        public string Definition;
        [JsonProperty("permalink")]
        public string Permalink;
        [JsonProperty("thumbs_up")]
        public int ThumbsUp;
        [JsonProperty("author")]
        public string Author;
        [JsonProperty("word")]
        public string Word;
        [JsonProperty("defid")]
        public int Defid;
        [JsonProperty("current_vote")]
        public string CurrentVote = "";
        [JsonProperty("example")]
        public string Example;
        [JsonProperty("thumbs_down")]
        public int ThumbsDown;

        [JsonConstructor]
        public UrbanResult()
        {
        }
    }
}