using System.Collections.Generic;

namespace GenericBot.Entities
{
    public class CustomCommand
    {
        public string Name;
        public List<string> Aliases;
        public bool Delete;
        public string Response;

        public CustomCommand()
        {

        }
        public CustomCommand(string name, string response)
        {
            this.Name = name;
            this.Aliases = new List<string>();
            this.Delete = true;
            this.Response = response;
        }
    }
}
