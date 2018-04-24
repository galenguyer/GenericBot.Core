using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Net.Queue;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class QuickCommands
    {
        protected class QuickCommand
        {
            private string _name;
            private string _returnValue;
            private string _description;
            public QuickCommand(string name, string returnValue, string description = "")
            {
                this._name = name;
                this._returnValue = returnValue;
                this._description = string.IsNullOrEmpty(description) ? null : description;
            }

            public Command GetCommand()
            {
                Command cmd = new Command(_name);
                cmd.SendTyping = false;
                cmd.Description = _description;
                cmd.ToExecute += async (client, msg, parameters) =>
                {
                    await msg.ReplyAsync(_returnValue);
                };

                return cmd;
            }
        }
        public List<Command> GetQuickCommands()
        {
            List<Command> quickCommands = new List<Command>();

            quickCommands.Add(new QuickCommand("touc", "https://i.imgur.com/3rYGu6V.jpg", "Posts a picture of a toucan").GetCommand());
            quickCommands.Add(new QuickCommand("justask", " If you have a question, don't ask if you can ask it. Just ask it, and someone will be along to help you as soon as they can", "Just ask copypaste").GetCommand());

            return quickCommands;
        }
    }
}
