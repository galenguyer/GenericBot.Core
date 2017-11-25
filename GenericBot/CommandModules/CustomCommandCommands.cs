using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            command.Aliases = new List<string>{"commands"};
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
                        rawResponse += $"`{cmd.Name}`: {cmd.Response.SafeSubstring(100)}\n";
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
                    parameters.RemoveRange(0, 2);
                    GenericBot.GuildConfigs[msg.GetGuild().Id].CustomCommands.Add(new CustomCommand(cName.ToLower(), parameters.reJoin()));
                    await msg.ReplyAsync($"New command created! \n```\n{JsonConvert.SerializeObject(new CustomCommand(cName.ToLower(), parameters.reJoin()),Formatting.Indented)}\n```");
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

            return CustomCommandCommands;
        }
    }
}
