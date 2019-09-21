using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GenericBot
{
    class GenericBot
    {
        private static GlobalConfiguration globalConfig;
        private static DiscordShardedClient DiscordClient;

        public static Logger Logger;
        public static string BuildId;
        static void Main(string[] args)
        {
            Logger = new Logger();
            globalConfig = new GlobalConfiguration().Load();
            DiscordClient = new DiscordShardedClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100,
            });

            if (File.Exists("version.txt"))
            {
                Logger.LogGenericMessage($"Build {File.ReadAllText("version.txt").Trim()}");
                BuildId = File.ReadAllText("version.txt").Trim();
            }

            Start().GetAwaiter().GetResult();
        }

        private static async Task Start()
        {
            // Hook into Discord Client events
            DiscordClient.Log += Logger.LogClientMessage;
            DiscordClient.MessageReceived += MessageEventHandler.MessageRecieved;

            try
            {
                await DiscordClient.LoginAsync(TokenType.Bot, globalConfig.DiscordToken);
                await DiscordClient.StartAsync();
            }
            catch (Exception e)
            {
                await Logger.LogErrorMessage($"{e.Message}\n{e.StackTrace}");
                return;
            }

            // Block until exited
            await Task.Delay(-1);
        }

        public static bool CheckBlacklisted(ulong UserId) =>        
            globalConfig.BlacklistedIds.Contains(UserId);
        public static ulong GetCurrentUserId() => DiscordClient.CurrentUser.Id;
        public static ulong GetOwnerId() => DiscordClient.GetApplicationInfoAsync().Result.Owner.Id;
        public static string GetPrefix() => globalConfig.DefaultPrefix;

    }
}
