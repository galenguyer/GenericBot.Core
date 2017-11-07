using Discord.WebSocket;

namespace GenericBot.Entities
{
    public class QueuedTweet
    {
        public SocketMessage msg;
        public ulong AuthorId;
        public string AuthorName;
        public ulong GuildId;
        public string GuildName;
        public ulong ChannelId;
        public string ChannelName;
        public string InputMessage;

        public QueuedTweet(SocketMessage _msg, string message)
        {
            this.msg = _msg;
            this.InputMessage = message;
        }

        public QueuedTweet()
        {

        }
    }
}
