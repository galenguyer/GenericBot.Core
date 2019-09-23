using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericBot.CommandModules
{
    public class InfoModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command ping = new Command("ping");
            ping.Description = "Make sure the bot is up";
            ping.ToExecute += async (context) =>
            {
                await context.Message.ReplyAsync("Pong!");
            };
            commands.Add(ping);

            return commands;
        }
    }
}
