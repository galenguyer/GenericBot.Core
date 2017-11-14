using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class RoleCommands
    {
        public List<Command> GetRoleCommands()
        {
            List<Command> RoleCommands = new List<Command>();

            Command UserRoles = new Command("userroles");
            UserRoles.Description = $"Show all user roles on this server";
            UserRoles.Usage = "userroles";
            UserRoles.ToExecute += async (client, msg, paramList) =>
            {
                string message = $"You can use `iam` and `iamnot` with any of these roles:\n";
                foreach (var role in msg.GetGuild().Roles
                    .Where(r => GenericBot.GuildConfigs[msg.GetGuild().Id].UserRoleIds.Contains(r.Id)))
                {
                    message += $"`{role.Name}`, ";
                }
                message = message.Trim(' ', ',');

                foreach (var str in message.SplitSafe())
                {
                    await msg.ReplyAsync(str);
                }
            };

            RoleCommands.Add(UserRoles);

            Command iam = new Command("iam");
            iam.Description = "Join a User Role";
            iam.Usage = "iam <role name>";
            iam.Aliases = new List<string>{"join"};
            iam.ToExecute += async (client, msg, paramList) =>
            {
                if (paramList.Empty())
                {
                    await msg.ReplyAsync($"Please select a role to join");
                }
                string input = paramList.Aggregate((i, j) => i + " " + j);

                var roles = msg.GetGuild().Roles.Where(r => r.Name.ToLower().Contains(input.ToLower()))
                    .Where(r => GenericBot.GuildConfigs[msg.GetGuild().Id].UserRoleIds.Contains(r.Id));

                if (!roles.Any())
                {
                    await msg.ReplyAsync($"Could not find any user roles matching `{input}`");
                }
                else if (roles.Count() == 1)
                {
                    try
                    {
                        RestUserMessage message;
                        if (msg.GetGuild().GetUser(msg.Author.Id).Roles.Any(r => r.Id == roles.First().Id))
                        {
                            message = await msg.ReplyAsync("You already have that role!");
                        }
                        else
                        {
                            await msg.GetGuild().GetUser(msg.Author.Id).AddRoleAsync(roles.First());
                            message = await msg.ReplyAsync("Done!");
                        }

                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                        await message.DeleteAsync();
                    }
                    catch (Exception e)
                    {
                        await GenericBot.Logger.LogErrorMessage(e.Message);
                        await msg.ReplyAsync($"I may not have permissions to do that!");
                    }
                }
                else if (roles.Count() > 1)
                {
                    await msg.ReplyAsync($"I found too many roles matching `{input}`");
                }
            };

            RoleCommands.Add(iam);

            Command iamnot = new Command("iamnot");
            iamnot.Description = "Leave a User Role";
            iamnot.Usage = "iamnot <role name>";
            iamnot.Aliases = new List<string>{"leave"};
            iamnot.ToExecute += async (client, msg, paramList) =>
            {
                if (paramList.Empty())
                {
                    await msg.ReplyAsync($"Please select a role to leave");
                }
                string input = paramList.Aggregate((i, j) => i + " " + j);

                var roles = msg.GetGuild().Roles.Where(r => r.Name.ToLower().Contains(input.ToLower()))
                    .Where(r => GenericBot.GuildConfigs[msg.GetGuild().Id].UserRoleIds.Contains(r.Id));

                if (!roles.Any())
                {
                    await msg.ReplyAsync($"Could not find any user roles matching `{input}`");
                }
                else if (roles.Count() == 1)
                {
                    try
                    {
                        RestUserMessage message;
                        if (!msg.GetGuild().GetUser(msg.Author.Id).Roles.Any(r => r.Id == roles.First().Id))
                        {
                            message = await msg.ReplyAsync("You don't have that role!");
                        }
                        else
                        {
                            await msg.GetGuild().GetUser(msg.Author.Id).RemoveRoleAsync(roles.First());
                            message = await msg.ReplyAsync("Done!");
                        }

                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                        await message.DeleteAsync();
                    }
                    catch (Exception e)
                    {
                        await GenericBot.Logger.LogErrorMessage(e.Message);
                        await msg.ReplyAsync($"I may not have permissions to do that!");
                    }
                }
                else if (roles.Count() > 1)
                {
                    await msg.ReplyAsync($"I found too many roles matching `{input}`");
                }
            };

            RoleCommands.Add(iamnot);

            Command getrole = new Command("getrole");
            getrole.Description = "Get the ID of a role";
            getrole.Usage = "getrole <role name>";
            getrole.RequiredPermission = Command.PermissionLevels.Moderator;
            getrole.ToExecute += async (client, msg, paramList) =>
            {
                string message = $"Roles matching `{paramList.reJoin()}`:\n";
                foreach (var role in msg.GetGuild().Roles.Where(r => r.Name.ToLower().Contains(paramList.reJoin())))
                {
                    message += $"{role.Name} (`{role.Id}`)\n";
                }

                foreach (var str in message.SplitSafe())
                {
                    msg.ReplyAsync(str);
                }
            };

            RoleCommands.Add(getrole);

            Command membersOf = new Command("membersof");
            membersOf.Description = "List all members of a role";
            membersOf.Usage = "membersof <rolename>";
            membersOf.RequiredPermission = Command.PermissionLevels.Moderator;
            membersOf.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync($"You need to specify a role");
                    return;
                }
                string result = "";
                foreach (var role in msg.GetGuild().Roles.OrderByDescending(r => r.Position).Where(r => r.Name.ToLower().Contains(parameters.reJoin())))
                {
                    result += $"\n**`{role.Name}` ({role.Members.Count()} Members)**\n";
                    foreach (var user in role.Members.OrderBy(u => u.Username))
                    {
                        if (!string.IsNullOrEmpty(user.Nickname)) result += $"{user.Nickname} ";
                        else result += $"{user.Username} ";
                        result += $"(`{user}`)\n";
                    }
                }

                foreach (var str in result.SplitSafe('\n'))
                {
                    await msg.ReplyAsync(str);
                }

            };

            RoleCommands.Add(membersOf);

            return RoleCommands;
        }
    }
}
