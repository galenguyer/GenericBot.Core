using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GenericBot
{
    class GenericBot
    {
        public static string BuildId;
        static void Main(string[] args)
        {
            if (File.Exists("version.txt"))
            {
                Core.Logger.LogGenericMessage($"Build {File.ReadAllText("version.txt").Trim()}");
                BuildId = File.ReadAllText("version.txt").Trim();
            }

            Start().GetAwaiter().GetResult();
        }

        private static async Task Start()
        {
            try
            {
                await Core.DiscordClient.LoginAsync(TokenType.Bot, Core.GlobalConfig.DiscordToken);
                await Core.DiscordClient.StartAsync();
            }
            catch (Exception e)
            {
                await Core.Logger.LogErrorMessage($"{e.Message}\n{e.StackTrace}");
                return;
            }

            // Block until exited
            await Task.Delay(-1);
        }
    }
}
