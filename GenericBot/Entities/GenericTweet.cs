using Discord.WebSocket;

namespace GenericBot.Entities
{
    public class GenericTweet
    {
        public ulong AuthorId;
        public string AuthorName;
        public ulong GuildId;
        public string GuildName;
        public ulong ChannelId;
        public string ChannelName;
        public bool Success;
        public string InputMessage;
        public string TweetUrl;

        public GenericTweet(SocketMessage msg, string message, string url, bool success)
        {
            this.AuthorId = msg.Author.Id;
            this.AuthorName = msg.Author.ToString();
            this.GuildId = msg.GetGuild().Id;
            this.GuildName = msg.GetGuild().ToString();
            this.ChannelId = msg.Channel.Id;
            this.ChannelName = msg.Channel.Name;
            this.InputMessage = message;
            this.TweetUrl = url;
            this.Success = success;
        }

        public GenericTweet()
        {

        }
    }
}
