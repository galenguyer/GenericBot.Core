using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot
{
    public static class MessageEventHandler
    {
        public static async Task MessageRecieved(SocketMessage parameterMessage, bool edited = false)
        {
            // Don't do stuff if the user is blacklisted
            if (GenericBot.CheckBlacklisted(parameterMessage.Author.Id))
                return;
            // Ignore self
            if (parameterMessage.Author.Id == GenericBot.GetCurrentUserId())
                return;
            try
            {
                var command = new Command("t").ParseMessage(parameterMessage);
                if(command != null)
                    await command.Execute();
            }
            catch (Exception ex)
            {
                if (parameterMessage.Author.Id == GenericBot.GetOwnerId())
                {
                    await parameterMessage.ReplyAsync("```\n" + $"{ex.Message}\n{ex.StackTrace}".SafeSubstring(1000) +
                                                      "\n```");
                }
                await GenericBot.Logger.LogErrorMessage(ex.Message);
                Console.WriteLine($"{ex.StackTrace}");
            }
        }

        public static async Task MessageRecieved(SocketMessage arg)
        {
            await MessageRecieved(arg, edited: false);
        }

    }
}