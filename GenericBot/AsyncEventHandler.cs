using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace GenericBot
{
    public static class AsyncEventHandler
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        public static async Task MessageRecieved(SocketMessage msg)
        {
            Task.Run(() => MessageEventHandler.MessageRecieved(msg));
        }
        public static async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            Task.Run(() => MessageEventHandler.HandleEditedCommand(before, after, channel));
        }
        public static async Task MessageDeleted(Cacheable<IMessage, ulong> msg, ISocketMessageChannel channel)
        {
            Task.Run(() => MessageEventHandler.MessageDeleted(msg, channel));
        }
        public static async Task UserJoinedGuild(SocketGuildUser user)
        {
            Task.Run(() => UserEventHandler.UserJoined(user));
        }
        public static async Task UserLeftGuild(SocketGuildUser user)
        {
            Task.Run(() => UserEventHandler.UserLeft(user));
        }
        public static async Task UserUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            Task.Run(() => UserEventHandler.UserUpdated(before, after));
        }
        public static async Task BotJoinedGuild(SocketGuild guild)
        {
            Task.Run(() => GuildEventHandler.OnJoinedGuild(guild));
        }
        public static async Task BotLeftGuild(SocketGuild guild)
        {
            Task.Run(() => GuildEventHandler.OnLeftGuild(guild));
        }
    }
}
