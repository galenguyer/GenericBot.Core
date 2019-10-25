using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericBot.CommandModules
{
    class GiveawayModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command giveaway = new Command("giveaway");
            giveaway.Usage = "giveaway [create <description>|join <id>|close <id>|roll <id>|list]";
            giveaway.ToExecute += async (context) =>
            {
                if(context.Parameters.Count == 0)
                {
                    await context.Message.ReplyAsync("Please choose one of the following options: `create`, `join`, `close`, or `roll`");
                    return;
                }

                if (context.Parameters[0].ToLower().Equals("create"))
                {
                    string desc = context.Parameters.Count > 1 ? context.ParameterString.Substring("create".Length).Trim() : string.Empty;
                    Giveaway newGiveaway = new Giveaway(context, desc);
                    newGiveaway = Core.CreateGiveaway(newGiveaway, context.Guild.Id);

                    await context.Message.ReplyAsync($"New giveaway with id `{newGiveaway.Id}` created!");
                }
                else if (context.Parameters[0].ToLower().Equals("join"))
                {

                }
                else if (context.Parameters[0].ToLower().Equals("close"))
                {

                }
                else if (context.Parameters[0].ToLower().Equals("roll"))
                {

                }
                else if (context.Parameters[0].ToLower().Equals("delete"))
                {

                }
                else if (context.Parameters[0].ToLower().Equals("list"))
                {

                }
            };
            commands.Add(giveaway);

            return commands;
        } 
    }
}
