using System;
using System.Collections.Generic;
using System.Linq;
using GenericBot.Entities;
using Newtonsoft.Json;

namespace GenericBot.CommandModules
{
    public class CustomCommandCommands
    {
        public List<Command> GetCustomCommands()
        {
            List<Command> CustomCommandCommands = new List<Command>();

            Command command = new Command("command");
            command.Aliases = new List<string> { "commands" };
            command.RequiredPermission = Command.PermissionLevels.Admin;
            command.Description = "Modify custom commands for a server";
            command.Usage = "command <list|add|remove|toggleDelete> [name] [response]";
            command.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync("You have to input an option");
                    return;
                }
                if (parameters[0].Equals("list"))
                {
                    if (!GenericBot.GuildConfigs[msg.GetGuild().Id].CustomCommands.Any())
                    {
                        await msg.ReplyAsync("This server has no custom commands");
                        return;
                    }
                    string rawResponse = "";
                    foreach (var cmd in GenericBot.GuildConfigs[msg.GetGuild().Id].CustomCommands)
                    {
                        string aliases = cmd.Aliases.Any() ? cmd.Aliases.reJoin(", ") : "None";
                        rawResponse += $"`{cmd.Name}`: {cmd.Response.SafeSubstring(100)}\nDelete: `{cmd.Delete}`, Aliases: `{aliases}`\n\n";
                    }
                    foreach (var resp in rawResponse.SplitSafe('\n'))
                    {
                        await msg.ReplyAsync(resp);
                    }
                }
                else if (parameters[0].Equals("add"))
                {
                    if (GenericBot.Commands.Any(c =>
                        c.Name.Equals(parameters[1]) || c.Aliases.Any(a => a.Equals(parameters[1]))) ||
                        GenericBot.GuildConfigs[msg.GetGuild().Id].CustomCommands.Any(c =>
                            c.Name.Equals(parameters[1]) || c.Aliases.Any(a => a.Equals(parameters[1]))))
                    {
                        await msg.ReplyAsync($"That's already a command, you can't add it.");
                        return;
                    }
                    string cName = parameters[1];
                    string pref = GenericBot.GlobalConfiguration.DefaultPrefix;
                    if (!String.IsNullOrEmpty(GenericBot.GuildConfigs[msg.GetGuild().Id].Prefix))
                        pref = GenericBot.GuildConfigs[msg.GetGuild().Id].Prefix;

                    string message = msg.Content;
                    message = message.Remove(0, pref.Length).TrimStart(' ').Remove(0, "command".Length).TrimStart('s').TrimStart(' ').Remove(0, "add".Length).TrimStart(' ').Remove(0, cName.Length).Trim(' ');

                    GenericBot.GuildConfigs[msg.GetGuild().Id].CustomCommands.Add(new CustomCommand(cName.ToLower(), message));
                    await msg.ReplyAsync($"New command created! \n```\n{JsonConvert.SerializeObject(new CustomCommand(cName.ToLower(), message), Formatting.Indented)}\n```");
                }
                else if (parameters[0].Equals("remove") || parameters[0].Equals("delete"))
                {
                    CustomCommand custom = new CustomCommand();
                    if (GenericBot.GuildConfigs[msg.GetGuild().Id].CustomCommands
                        .HasElement(c => c.Name == parameters[1], out custom))
                    {
                        GenericBot.GuildConfigs[msg.GetGuild().Id].CustomCommands.Remove(custom);
                        await msg.ReplyAsync($"The command `{custom.Name}` has been deleted");
                    }
                    else
                    {
                        await msg.ReplyAsync($"No command matching `{parameters[1]}` exists");
                    }
                }
                else if (parameters[0].ToLower().Equals("toggledelete"))
                {
                    CustomCommand custom = new CustomCommand();
                    if (GenericBot.GuildConfigs[msg.GetGuild().Id].CustomCommands
                        .HasElement(c => c.Name == parameters[1], out custom))
                    {
                        custom.Delete = !custom.Delete;
                        if (!custom.Delete)
                        {
                            await msg.ReplyAsync($"The command `{custom.Name}` will **not** be deleted");
                        }
                        else
                        {
                            await msg.ReplyAsync($"The command `{custom.Name}` **will** be deleted");
                        }
                    }
                    else
                    {
                        await msg.ReplyAsync($"No command matching `{parameters[1]}` exists");
                    }
                }
                GenericBot.GuildConfigs[msg.GetGuild().Id].Save();
            };

            CustomCommandCommands.Add(command);

            Command alias = new Command("alias");
            alias.Aliases = new List<string> { "aliases" };
            alias.RequiredPermission = Command.PermissionLevels.Admin;
            alias.Description = "Add an alias for a command";
            alias.Usage = "alias <add|remove> command alias";
            alias.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync("You need to have some options");
                }

                if (string.IsNullOrEmpty(parameters[1]) ||
                    (!GenericBot.Commands.Any(c => c.Name.Equals(parameters[1]) || c.Aliases.Contains(parameters[1])) &&
                     !GenericBot.GuildConfigs[msg.GetGuild().Id].CustomCommands.Any(c =>
                         c.Name.Equals(parameters[1]) || c.Aliases.Contains(parameters[1]))))
                {
                    await msg.ReplyAsync("That's not a command");
                    return;
                }

                if (parameters[0].ToLower().Equals("add"))
                {
                    if (string.IsNullOrEmpty(parameters[2]) ||
                        (GenericBot.Commands.Any(c => c.Name.Equals(parameters[2]) || c.Aliases.Contains(parameters[2])) ||
                         GenericBot.GuildConfigs[msg.GetGuild().Id].CustomCommands.Any(c =>
                             c.Name.Equals(parameters[2]) || c.Aliases.Contains(parameters[2]))))
                    {
                        await msg.ReplyAsync($"That's already an alias or command");
                        return;
                    }
                    CustomCommand custom = new CustomCommand();
                    if (GenericBot.GuildConfigs[msg.GetGuild().Id].CustomCommands.HasElement(c =>
                        c.Name.Equals(parameters[1]) || c.Aliases.Contains(parameters[1]), out custom))
                    {
                        custom.Aliases.Add(parameters[2].ToLower());
                        await msg.ReplyAsync(
                            $"The command `{custom.Name}` now has the alias `{parameters[2].ToLower()}`");
                    }
                    else
                    {
                        if (GenericBot.GuildConfigs[msg.GetGuild().Id].CustomAliases.Any(a =>
                            a.Command.Equals(parameters[1]) && a.Alias.Equals(parameters[2])))
                        {
                            await msg.ReplyAsync("That already exists");
                            return;
                        }
                        GenericBot.GuildConfigs[msg.GetGuild().Id].CustomAliases.Add(new CustomAlias(parameters[1], parameters[2].ToLower()));
                        await msg.ReplyAsync($"The command `{parameters[1]}` now has the alias `{parameters[2]}`");
                    }
                }
                else if (parameters[0].ToLower().Equals("remove"))
                {
                    CustomCommand custom = new CustomCommand();
                    if (GenericBot.GuildConfigs[msg.GetGuild().Id].CustomCommands.HasElement(c =>
                        c.Name.Equals(parameters[1]) || c.Aliases.Contains(parameters[1]), out custom))
                    {
                        custom.Aliases.Remove(parameters[2].ToLower());
                        await msg.ReplyAsync("Done");
                    }
                    else
                    {
                        GenericBot.GuildConfigs[msg.GetGuild().Id].CustomAliases.Remove(
                            GenericBot.GuildConfigs[msg.GetGuild().Id].CustomAliases.First(a => a.Command.Equals(parameters[1]) && a.Alias.Equals(parameters[2])));
                        await msg.ReplyAsync("Done");
                    }
                }
                else
                {
                }
                GenericBot.GuildConfigs[msg.GetGuild().Id].Save();
            };

            CustomCommandCommands.Add(alias);

            return CustomCommandCommands;
        }
    }
}
