using System.Collections.Generic;
using System.Linq;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class HelpModule
    {
        public List<Command> GetHelpCommands()
        {
            List<Command> helpCommands = new List<Command>();

            Command help = new Command("help");
            help.Description = "The help command, duh";
            help.RequiredPermission = Command.PermissionLevels.User;
            help.Aliases = new List<string> {"halp"};
            help.ToExecute += async (client, msg, paramList) =>
            {
                string commands= "";
                var guildCustomCommands = GenericBot.GuildConfigs[msg.GetGuild().Id].CustomCommands;

                if(paramList.Empty())
                {
                    foreach (var cmd in GenericBot.Commands.Where(c => c.GetPermissions(msg.Author, msg.GetGuild().Id) >= c.RequiredPermission).OrderBy(c => c.RequiredPermission))
                    {
                        commands += $"`{cmd.Name}`: {cmd.Description}\n";
                    }

                    if (guildCustomCommands.Any())
                    {
                        foreach (var cmd in guildCustomCommands)
                        {
                            commands += $"`{cmd.Name}`: Custom Command\n";
                        }
                    }
                }

                else
                {
                    foreach (var cmd in GenericBot.Commands.Where(c => c.GetPermissions(msg.Author, msg.GetGuild().Id) >= c.RequiredPermission).Where(c => c.Name.Contains(paramList[0])).OrderBy(c => c.RequiredPermission))
                    {
                        commands += $"`{cmd.Name}`: {cmd.Description} (`{cmd.Usage}`)\n";
                        if(!cmd.Aliases.Empty() || !GenericBot.GuildConfigs[msg.GetGuild().Id].CustomAliases.Where(a => a.Command.Equals(cmd.Name)).Select(a => a.Alias).ToList().Empty())
                        {
                            List<string> aliases = cmd.Aliases;
                            aliases.AddRange(GenericBot.GuildConfigs[msg.GetGuild().Id].CustomAliases.Where(a => a.Command.Equals(cmd.Name)).Select(a => a.Alias));
                            commands += $"\tAliases: `{aliases.Aggregate((i, j) => i + ", " + j)}`\n";
                        }
                    }
                    if (guildCustomCommands.Any())
                    {
                        foreach (var cmd in guildCustomCommands.Where(c => c.Name.Contains(paramList[0]) || c.Aliases.Any(a => a.Contains(paramList[0]))))
                        {
                            commands += $"`{cmd.Name}`: Custom Command\n";
                            if (cmd.Aliases.Any())
                            {
                                commands += $"\tAliases: `{cmd.Aliases.Aggregate((i, j) => i + ", " + j)}`\n";
                            }
                        }
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
    }
}
