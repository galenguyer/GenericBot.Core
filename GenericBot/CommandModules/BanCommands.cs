using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class BanCommands
    {
        public List<Command> GetBanCommands()
        {
            List<Command> banCommands = new List<Command>();

            Command globalBan = new Command("globalBan");
            globalBan.RequiredPermission = Command.PermissionLevels.BotOwner;
            globalBan.Description = "Ban someone from every server the bot is currently on";
            globalBan.ToExecute += async (client, msg, parameters) =>
            {
                if (!ulong.TryParse(parameters[0], out ulong userId))
                {
                    await msg.ReplyAsync($"Invalid UserId");
                    return;
                }

                if (parameters.Count <= 1)
                {
                    await msg.ReplyAsync($"Need a reasono and/or userId");
                    return;
                }

                string reason = $"Globally banned for: {parameters.reJoin()}";

                int succ = 0, fail= 0 , opt = 0;
                GenericBot.GlobalConfiguration.BlacklistedIds.Add(userId);
                foreach (var guild in client.Guilds)
                {
                    if (GenericBot.GuildConfigs[guild.Id].GlobalBanOptOut)
                    {
                        opt++;
                        continue;
                    }

                    try
                    {
                        await guild.AddBanAsync(userId, 0, reason);
                        succ++;
                    }
                    catch
                    {
                        fail++;
                    }
                }

                string repl =
                    $"Result: Banned `{msg.GetGuild().GetBansAsync().Result.First(b => b.User.Id == userId).User}` for `{reason}`\n";
                repl += $"\nSuccesses: `{succ}`\nFailures: `{fail}`\nOpt-Outs: `{opt}`";

                await msg.ReplyAsync(repl);

            };

            banCommands.Add(globalBan);

            Command ban = new Command("ban");
            ban.Description = "Ban a user from the server, whether or not they're on it";
            ban.Delete = false;
            ban.RequiredPermission = Command.PermissionLevels.Moderator;
            ban.Usage = $"{ban.Name} <user> <time in days> <reason>";
            ban.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync($"You need to add some arguments. A user, perhaps?");
                    return;
                }

                ulong uid;
                if (ulong.TryParse(parameters[0].TrimStart('<', '@', '!').TrimEnd('>'), out uid))
                {
                    parameters.RemoveAt(0);
                    int time = 0;

                    if (int.TryParse(parameters[0].TrimEnd('d'), out time))
                    {
                        parameters.RemoveAt(0);
                    }

                    string reason = parameters.reJoin();

                    var bans = msg.GetGuild().GetBansAsync().Result;

                    if (bans.Any(b => b.User.Id == uid))
                    {
                        await msg.ReplyAsync(
                            $"`{bans.First(b => b.User.Id == uid).User}` is already banned for `{bans.First(b => b.User.Id == uid).Reason}`");
                    }
                    else
                    {
                        bool dmSuccess = true;
                        string dmMessage = $"You have been banned from **{msg.GetGuild().Name}** ";
                        dmMessage += time == 0 ? "permanently" : $"for `{time}` days";
                        if(!string.IsNullOrEmpty(reason))
                            dmMessage += $" for the following reason: \n\n{reason}\n\n";
                        try
                        {
                            await msg.GetGuild().GetUser(uid).GetOrCreateDMChannelAsync().Result
                                .SendMessageAsync(dmMessage);
                        }
                        catch
                        {
                            dmSuccess = false;
                        }

                        await msg.GetGuild().AddBanAsync(uid);
                        bans = msg.GetGuild().GetBansAsync().Result;
                        var user = bans.First(u => u.User.Id == uid).User;
                        string banMessage = $"Banned `{user}` (`{user.Id}`)";
                        if (string.IsNullOrEmpty(reason))
                            banMessage += $" 👌";
                        else
                            banMessage += $" for `{reason}`";
                        banMessage += time == 0 ? $" permanently 👌" : $" for `{time}` days 👌";

                        if (!dmSuccess) banMessage += "\nThe user could not be messaged";

                        var builder = new EmbedBuilder()
                            .WithTitle("User Banned")
                            .WithDescription(banMessage)
                            .WithColor(new Color(0xFFFF00))
                            .WithFooter(footer => {
                                footer
                                    .WithText($"By {msg.Author} at {DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")} GMT");
                            })
                            .WithAuthor(author => {
                                author
                                    .WithName(user.ToString())
                                    .WithIconUrl(user.GetAvatarUrl());
                            });

                        var guilddb = new DBGuild(msg.GetGuild().Id);
                        var guildconfig = GenericBot.GuildConfigs[msg.GetGuild().Id];
                        guildconfig.Bans.Add(
                            new GenericBan(user.Id, msg.GetGuild().Id, reason, time));
                        guildconfig.ProbablyMutedUsers.Remove(user.Id);
                        string t = time == 0 ? "permanently" : $"for `{time}` days";
                        guildconfig.Save();
                        guilddb.GetUser(user.Id)
                            .AddWarning(
                                $"Banned {t} for `{reason}` (By `{msg.Author}` At `{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")} GMT`)");
                        guilddb.Save();

                        await msg.Channel.SendMessageAsync("", embed: builder.Build());
                        if (guildconfig.UserLogChannelId != 0)
                        {
                            await (client.GetChannel(guildconfig.UserLogChannelId) as SocketTextChannel)
                                .SendMessageAsync("", embed: builder.Build());
                        }
                    }
                }
            };

            banCommands.Add(ban);

            return banCommands;
        }
    }
}
