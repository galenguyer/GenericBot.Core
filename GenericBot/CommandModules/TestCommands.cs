using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class TestCommands
    {
        public List<Command> GetTestCommands()
        {
            List<Command> TestCommands = new List<Command>();

            Command test = new Command("test");
            test.Delete = false;
            test.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            test.ToExecute += async (client, msg, paramList) =>
            {

            };

            TestCommands.Add(test);

            return TestCommands;
        }
    }
}
