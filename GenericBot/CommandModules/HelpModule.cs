using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class HelpModule
    {
        public List<Command> GetHelpCommands()
        {
            List<Command> helpCommands = new List<Command>();

            Command help = new Command();
            help.Name = "help";
            help.Description = "The help command, duh";
            help.RequiredPermission = Command.PermissionLevels.User;
            help.Aliases = new List<string> {"halp"};
            help.ToExecute += async (client, msg, paramList) =>
            {
                string commands= "";

                if(!paramList.Any())
                {
                    foreach (var cmd in GenericBot.Commands.Where(c => GetPermissions(msg.Author) >= c.RequiredPermission))
                    {
                        commands += $"`{cmd.Name}`: {cmd.Description}\n";
                    }
                }

                else
                {
                    foreach (var cmd in GenericBot.Commands.Where(c => GetPermissions(msg.Author) >= c.RequiredPermission).Where(c => c.Name.Contains(paramList[0])))
                    {
                        commands += $"`{cmd.Name}`: {cmd.Description} (`{cmd.Usage}`)\n";
                    }
                }
                foreach (var str in commands.SplitSafe())
                {
                    await msg.Channel.SendMessageAsync(str);
                }
            };
            helpCommands.Add(help);

            return helpCommands;
        }
        private Command.PermissionLevels GetPermissions(SocketUser user)
        {
            if (user.Id.Equals(GenericBot.GlobalConfiguration.OwnerId)) return Command.PermissionLevels.BotOwner;
            else if (GenericBot.GlobalConfiguration.GlobalAdminIds.Contains(user.Id))
                return Command.PermissionLevels.GlobalAdmin;
            else return Command.PermissionLevels.User;
        }
    }
}
