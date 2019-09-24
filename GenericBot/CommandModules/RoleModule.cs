using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class RoleModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commmands = new List<Command>();

            Command UserRoles = new Command("userroles");
            UserRoles.Description = $"Show all user roles on this server";
            UserRoles.Usage = "userroles";
            UserRoles.ToExecute += async (context) =>
            {
                string prefix = Core.GetPrefix(context);
                string message = $"You can use `{prefix}iam` and `{prefix}iamnot` with any of these roles:\n _\\*(Pro tip: You can add/remove more than one role at a time by seperating each role with a comma like `{prefix}iam role0, role1, role2, etc`)*_\n";
                var config = Core.GetGuildConfig(context.Guild.Id);
                foreach (var group in config.UserRoles.Where(g => !string.IsNullOrEmpty(g.Key) && g.Value.Count > 0).OrderBy(g => g.Key))
                {
                    message += $"**{group.Key}:** ";
                    foreach (var role in context.Guild.Roles
                        .Where(r => group.Value.Contains(r.Id))
                        .OrderBy(r => r.Name))
                    {
                        if ((context.Author as SocketGuildUser).Roles.Contains(role))
                            message += "\\✔ ";
                        else
                            message += "✘";
                        message += $"`{role.Name}`, ";
                    }
                    message += "\n";
                }

                if (config.UserRoles[""].Count > 0)
                {
                    message += $"**Ungrouped:** ";
                    foreach (var role in context.Guild.Roles
                        .Where(r => config.UserRoles[""].Contains(r.Id))
                        .OrderBy(r => r.Name))
                    {
                        if ((context.Author as SocketGuildUser).Roles.Contains(role))
                            message += "\\✔ ";
                        else
                            message += "✘";
                        message += $"`{role.Name}`, ";
                    }
                    message += "\n";
                }

                message = message.Trim(' ', ',', '\n');
                message += $"\n You can also use `{prefix}rolestore save` to backup your assigned roles";

                foreach (var str in message.SplitSafe())
                {
                    await context.Message.ReplyAsync(str);
                }
            };
            commmands.Add(UserRoles);

            return commmands;
        }
    }
}
