using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericBot.CommandModules
{
    class CustomCommandModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command command = new Command("command");


            return commands;
        }
    }
}
