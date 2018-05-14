using System.Collections.Generic;
using Discord;

namespace GenericBot.Entities
{
    public class ParsedCommand
    {
        public Command Command;
        public string Name;
        public List<string> Parameters;
        public IMessage Message;
    }
}
