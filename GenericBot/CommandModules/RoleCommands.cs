using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
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
                string prefix = (!String.IsNullOrEmpty(GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix))
                ? GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix : GenericBot.GlobalConfiguration.DefaultPrefix;
                string message = $"You can use `{prefix}iam` and `{prefix}iamnot` with any of these roles:\n";
                foreach (var role in msg.GetGuild().Roles
                    .Where(r => GenericBot.GuildConfigs[msg.GetGuild().Id].UserRoleIds.Contains(r.Id))
                    .OrderByDescending(r => r.Position))
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
                IMessage rep;
                if (paramList.Empty())
                {
                    rep = msg.ReplyAsync($"Please select a role to join").Result;
                    GenericBot.QueueMessagesForDelete(new List<IMessage>{msg, rep});
                }
                string input = paramList.Aggregate((i, j) => i + " " + j);

                var roles = msg.GetGuild().Roles.Where(r => r.Name.ToLower().Contains(input.ToLower()))
                    .Where(r => GenericBot.GuildConfigs[msg.GetGuild().Id].UserRoleIds.Contains(r.Id));

                if (!roles.Any())
                {
                    rep = msg.ReplyAsync($"Could not find any user roles matching `{input}`").Result;
                    GenericBot.QueueMessagesForDelete(new List<IMessage>{msg, rep});
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
                    try
                    {
                        var role = roles.Any(r => r.Name.ToLower() == input.ToLower())
                            ? roles.First(r => r.Name.ToLower() == input.ToLower())
                            : roles.First();
                            RestUserMessage message;
                        if (msg.GetGuild().GetUser(msg.Author.Id).Roles.Any(r => r.Id == roles.First().Id))
                        {
                            message = await msg.ReplyAsync("You already have that role!");
                        }
                        else
                        {
                            await msg.GetGuild().GetUser(msg.Author.Id).AddRoleAsync(role);
                            message = await msg.ReplyAsync($"I've assigned you `{role.Name}`");
                        }

                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                        await message.DeleteAsync();
                    }
                    catch (Exception e)
                    {
                        await GenericBot.Logger.LogErrorMessage(e.Message);
                        await msg.ReplyAsync($"I may not have permissions to do that!");
                    }                }
            };

            RoleCommands.Add(iam);

            Command iamnot = new Command("iamnot");
            iamnot.Description = "Leave a User Role";
            iamnot.Usage = "iamnot <role name>";
            iamnot.Aliases = new List<string>{"leave"};
            iamnot.ToExecute += async (client, msg, paramList) =>
            {
                IMessage rep;
                if (paramList.Empty())
                {
                    rep =  msg.ReplyAsync($"Please select a role to leave").Result;
                    GenericBot.QueueMessagesForDelete(new List<IMessage>{msg, rep});
                }
                string input = paramList.Aggregate((i, j) => i + " " + j);

                var roles = msg.GetGuild().Roles.Where(r => r.Name.ToLower().Contains(input.ToLower()))
                    .Where(r => GenericBot.GuildConfigs[msg.GetGuild().Id].UserRoleIds.Contains(r.Id));

                if (!roles.Any())
                {
                    rep = msg.ReplyAsync($"Could not find any user roles matching `{input}`").Result;
                    GenericBot.QueueMessagesForDelete(new List<IMessage>{msg, rep});
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
                            message = await msg.ReplyAsync($"Removed `{roles.First()}`");
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
                foreach (var role in msg.GetGuild().Roles.OrderByDescending(r => r.Position).Where(r => new Regex(parameters.reJoin(), RegexOptions.IgnoreCase).IsMatch(r.Name) && r.Name != "@everyone"))
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

            Command createRole = new Command("createRole");
            createRole.Description = "Create a new role with default permissions";
            createRole.Usage = "createRole <name>";
            createRole.RequiredPermission = Command.PermissionLevels.Admin;
            createRole.ToExecute += async (client, msg, parameters) =>
            {

                var role = msg.GetGuild().CreateRoleAsync(parameters.reJoin(), GuildPermissions.None).Result;

                await msg.ReplyAsync($"Created new role `{role.Name}` with ID `{role.Id}`");
            };

            RoleCommands.Add(createRole);

            return RoleCommands;
        }
    }
}
