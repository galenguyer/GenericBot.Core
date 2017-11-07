using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class TestCommands
    {
        public List<Command> GetTestCommands()
        {
            List<Command> TestCommands = new List<Command>();

            Command test = new Command("getperms");
            test.Delete = true;
            //test.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            test.ToExecute += async (client, msg, paramList) =>
            {
                await msg.Channel.SendMessageAsync($"{test.GetPermissions(msg.Author, msg.GetGuild().Id)}");
            };

            TestCommands.Add(test);

            return TestCommands;
        }
    }
}
