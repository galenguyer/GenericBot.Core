using Discord.WebSocket;
using GenericBot.Entities;
using System;
using System.IO;

namespace GenericBot
{
    class GenericBot
    {
        private static GlobalConfiguration globalConfig;
        private static DiscordShardedClient discordClient;

        public static Logger Logger;
        public static string BuildId;
        static void Main(string[] args)
        {
            Logger = new Logger();
            globalConfig = new GlobalConfiguration().Load();

            if (File.Exists("version.txt"))
            {
                Logger.LogGenericMessage($"Build {File.ReadAllText("version.txt").Trim()}");
                BuildId = File.ReadAllText("version.txt").Trim();
            }
        }
    }
}
