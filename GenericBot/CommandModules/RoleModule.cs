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
                //message += $"\n You can also use `{prefix}rolestore save` to backup your assigned roles";

                foreach (var str in message.SplitSafe())
                {
                    await context.Message.ReplyAsync(str);
                }
            };
            commmands.Add(UserRoles);

            Command iam = new Command("iam");
            iam.Description = "Join a User Role";
            iam.Usage = "iam <role name>";
            iam.Aliases = new List<string> { "join" };
            iam.ToExecute += async (context) =>
            {
                List<IMessage> messagesToDelete = new List<IMessage>();

                if (context.Parameters.IsEmpty())
                {
                    messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription("Please select a role to join").WithColor(new Color(0xFFFF00)).Build()).Result);
                }

                foreach (var roleName in context.ParameterString.Trim(',', ' ').Split(','))
                {
                    if (string.IsNullOrWhiteSpace(roleName))
                        continue;
                    var roles = context.Guild.Roles.Where(r => r.Name.ToLower().Contains(roleName.ToLower().Trim()))
                        .Where(r => Core.GetGuildConfig(context.Guild.Id).UserRoles.Any(rg => rg.Value.Contains(r.Id)));
                    if (!roles.Any())
                    {
                        messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"Could not find any user roles matching `{roleName}`").WithColor(new Color(0xFFFF00)).Build()).Result);
                    }
                    else
                    {
                        try
                        {
                            var role = roles.Any(r => r.Name.ToLower() == roleName.ToLower())
                                ? roles.First(r => r.Name.ToLower() == roleName.ToLower())
                                : roles.First();
                            if (context.Guild.GetUser(context.Author.Id).Roles.Any(r => r.Id == roles.First().Id))
                            {
                                messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"You already have that role!").WithColor(new Color(0xFFFF00)).Build()).Result);
                            }
                            else
                            {
                                await context.Guild.GetUser(context.Author.Id).AddRoleAsync(role);
                                messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"Assigned you `{role.Name}`").WithColor(new Color(0x9B00)).Build()).Result);
                            }
                        }
                        catch (Exception e)
                        {
                            await Core.Logger.LogErrorMessage(e.Message);
                            messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"I may not have permissions to do that").WithColor(new Color(0xFFFF00)).Build()).Result);
                        }
                    }
                }

                await Task.Delay(15 * 1000);
                try
                {
                    messagesToDelete.ForEach(m => GenericBot.ClearedMessageIds.Add(m.Id));
                    messagesToDelete.Add(context.Message);
                    await (context.Channel as ITextChannel).DeleteMessagesAsync(messagesToDelete);
                }
                catch
                {
                    try
                    {
                        foreach (var m in messagesToDelete)
                        {
                            await m.DeleteAsync();
                        }
                    }
                    catch { }
                }
            };
            commmands.Add(iam);

            Command iamnot = new Command("iamnot");
            iamnot.Description = "Leave a User Role";
            iamnot.Usage = "iamnot <role name>";
            iamnot.Aliases = new List<string> { "leave" };
            iamnot.ToExecute += async (context) =>
            {
                List<IMessage> messagesToDelete = new List<IMessage>();

                if (context.Parameters.IsEmpty())
                {
                    messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription("Please select a role to remove").WithColor(new Color(0xFFFF00)).Build()).Result);
                }

                foreach (var roleName in context.ParameterString.Trim(',', ' ').Split(','))
                {
                    if (string.IsNullOrWhiteSpace(roleName))
                        continue;
                    var roles = context.Guild.Roles.Where(r => r.Name.ToLower().Contains(roleName.ToLower().Trim()))
                        .Where(r => Core.GetGuildConfig(context.Guild.Id).UserRoles.Any(rg => rg.Value.Contains(r.Id)));
                    if (!roles.Any())
                    {
                        messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"Could not find any user roles matching `{roleName}`").WithColor(new Color(0xFFFF00)).Build()).Result);
                    }
                    else
                    {
                        try
                        {
                            var role = roles.Any(r => r.Name.ToLower() == roleName.ToLower())
                                ? roles.First(r => r.Name.ToLower() == roleName.ToLower())
                                : roles.First();
                            if (!context.Guild.GetUser(context.Author.Id).Roles.Any(r => r.Id == roles.First().Id))
                            {
                                messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"You don't have that role!").WithColor(new Color(0xFFFF00)).Build()).Result);
                            }
                            else
                            {
                                await context.Guild.GetUser(context.Author.Id).RemoveRoleAsync(role);
                                messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"Removed `{role.Name}`").WithColor(new Color(0x9B00)).Build()).Result);
                            }
                        }
                        catch (Exception e)
                        {
                            await Core.Logger.LogErrorMessage(e.Message);
                            messagesToDelete.Add(context.Channel.SendMessageAsync("", embed: new EmbedBuilder().WithDescription($"I may not have permissions to do that").WithColor(new Color(0xFFFF00)).Build()).Result);
                        }
                    }
                }

                await Task.Delay(15 * 1000);
                try
                {
                    messagesToDelete.ForEach(m => GenericBot.ClearedMessageIds.Add(m.Id));
                    messagesToDelete.Add(context.Message);
                    await (context.Channel as ITextChannel).DeleteMessagesAsync(messagesToDelete);
                }
                catch
                {
                    try
                    {
                        foreach (var m in messagesToDelete)
                        {
                            await m.DeleteAsync();
                        }
                    }
                    catch { }
                }
            };
            commmands.Add(iamnot);
            return commmands;
        }
    }
}
