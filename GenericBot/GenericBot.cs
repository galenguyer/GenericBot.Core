using Discord.WebSocket;
using GenericBot.Entities;
using System;

namespace GenericBot
{
    class GenericBot
    {
        private static GlobalConfiguration globalConfig;
        private static DiscordShardedClient discordClient;
        public static Logger Logger;
        static void Main(string[] args)
        {
            Logger = new Logger();
            globalConfig = new GlobalConfiguration().Load();

        }
    }
}
