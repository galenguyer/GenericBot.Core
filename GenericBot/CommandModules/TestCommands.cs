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

            Command test = new Command();
            test.Name = "test";
            test.Delete = true;
            test.Aliases = new List<string>{"testcommand"};
            test.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            test.ToExecute += async (client, msg, paramList) =>
            {
                await msg.Channel.SendMessageAsync(paramList.Aggregate((i, j) => i + " " + j));
            };

            TestCommands.Add(test);

            return TestCommands;
        }
    }
}
