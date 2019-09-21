using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace GenericBot.Entities
{
    public class ParsedCommand
    {
        public Command RawCommand;
        public string Name;
        public List<string> Parameters;
        public string ParameterString;
        public IMessage Message;

        public Task Execute()
        {
            return RawCommand.ExecuteCommand(this);
        }
    }
}
