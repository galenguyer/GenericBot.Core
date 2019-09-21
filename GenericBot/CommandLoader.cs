using GenericBot.CommandModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GenericBot
{
    class CommandLoader
    {
        public void Load()
        {
            GenericBot.Commands.AddRange(new BaseCommands().Load());
        }
    }
}
