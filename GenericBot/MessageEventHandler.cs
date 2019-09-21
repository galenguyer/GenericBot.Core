using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace GenericBot
{
    public static class MessageEventHandler
    {
        public static async Task MessageRecieved(SocketMessage parameterMessage, bool edited = false)
        {
            // Don't handle the command if it is a system message
            var message = parameterMessage as IUserMessage;

            // Don't do stuff if the user is blacklisted
            if (GenericBot.CheckBlacklisted(message.Author.Id))
                return;
            // Ignore self
            if (parameterMessage.Author.Id == GenericBot.GetCurrentUserId())
                return;
            try
            {
                if (message.Content.ToLower().Equals($"{GenericBot.GetPrefix()}ping"))
                    await parameterMessage.ReplyAsync("Pong!");
            }
            //catch (NullReferenceException nullRefEx)
            //{
            //    Console.WriteLine($"Probably ignore nullref: \n{nullRefEx.StackTrace}"); 
            //}
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
    }
}