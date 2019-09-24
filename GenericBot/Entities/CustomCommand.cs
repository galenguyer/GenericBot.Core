using System.Collections.Generic;

namespace GenericBot.Entities
{
    public class CustomCommand
    {
        public string Name { get; set; }
        public List<string> Aliases { get; set; }
        public bool Delete { get; set; }
        public string Response { get; set; }

        public CustomCommand()
        {

        }
        public CustomCommand(string name, string response)
        {
            this.Name = name;
            this.Aliases = new List<string>();
            this.Delete = false;
            this.Response = response;
        }
    }
}
