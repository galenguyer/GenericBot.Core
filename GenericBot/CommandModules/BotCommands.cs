using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Discord;
using Discord.Commands;
using Discord.Net.Queue;
using Discord.WebSocket;
using GenericBot.Entities;
using Hammock.Web;
using Newtonsoft.Json;
using TweetSharp;
using ThreadState = System.Diagnostics.ThreadState;

namespace GenericBot.CommandModules
{
    public class BotCommands
    {
        public List<Command> GetBotCommands()
        {
            List<Command> botCommands = new List<Command>();

            Command ping = new Command("ping");
            ping.Description = $"Get the ping time to the bot";
            ping.Usage = $"ping <verbose>";
            ping.RequiredPermission = Command.PermissionLevels.Moderator;
            ping.ToExecute += async (client, msg, paramList) =>
            {
                var stop = new Stopwatch();
                stop.Start();
                GenericBot.QuickWatch.Stop();
                var rep = await msg.Channel.SendMessageAsync("Pong!");

                if (paramList.FirstOrDefault() != null && paramList.FirstOrDefault().Equals("verbose"))
                {
                    stop.Stop();
                    await rep.ModifyAsync(m => m.Content = $"Pong!\nProcess: `{GenericBot.QuickWatch.ElapsedMilliseconds}ms` \nAck + Resp: `{stop.ElapsedMilliseconds}ms`");
                }
            };
            botCommands.Add(ping);

            Command global = new Command("global");
            global.Description = "Get the global information for the bot";
            global.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            global.ToExecute += async (client, msg, paramList) =>
            {
                string stats = $"**Global Stats:** `{DateTime.Now}`\n" +
                               $"SessionID: `{GenericBot.Logger.SessionId}`\n" +
                               $"Build Number: `{GenericBot.BuildNumber}`\n\n" +
                               $"Servers: `{client.Guilds.Count}`\n" +
                               $"Users: `{client.Guilds.Sum(g => g.Users.Count)}`\n" +
                               $"Shards: `{client.Shards.Count}`\n" +
                              // $"CPU Usage: `{Math.Round(GenericBot.CpuCounter.NextValue())}`% \n" +
                               $"Memory: `{Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB`\n" +
                               $"Uptime: `{(DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss")}`\n" +
                               $"Disconnects: `{GenericBot.Disconnects}`\n\n";

                foreach (var shard in GenericBot.DiscordClient.Shards)
                {
                    stats += $"Shard `{shard.ShardId}`: `{shard.Guilds.Count}` Guilds (`{shard.Guilds.Sum(g => g.Users.Count)}` Users)\n";
                }

                await msg.Channel.SendMessageAsync(stats);
            };
            botCommands.Add(global);

            Command info = new Command("info");
            info.Description = "Send an informational card about the bot";
            info.ToExecute += async (client, msg, parameters) =>
            {
                string prefix = GenericBot.GlobalConfiguration.DefaultPrefix;
                if (!String.IsNullOrEmpty(GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix))
                    prefix = GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix;

                string config = info.GetPermissions(msg.Author, msg.GetGuild().Id) >= Command.PermissionLevels.Admin ? $" Admins can also run `{prefix}confighelp` to see everything you can set up" : "";

                var builder = new EmbedBuilder()
                    .WithTitle("GenericBot: An All-Purpose Almost-Decent Bot")
                    .WithDescription("GenericBot aims to provide an almost full featured moderation and fun box experience in one convenient package")
                    .WithUrl("https://github.com/MasterChief-John-117/GenericBot")
                    .WithColor(new Color(0xFF))
                    .WithFooter(footer =>
                    {
                        footer
                            .WithText($"Made by {GenericBot.DiscordClient.GetApplicationInfoAsync().Result.Owner.ToString()} | Hosted by {GenericBot.DiscordClient.GetUser(152905790959779840).ToString()}")
                            .WithIconUrl(GenericBot.DiscordClient.GetApplicationInfoAsync().Result.Owner.GetAvatarUrl());
                    })
                    .WithThumbnailUrl(GenericBot.DiscordClient.CurrentUser.GetAvatarUrl().Replace("size=128", "size=2048"))
                    .AddField($"Links", $"GenericBot is currently in a closed state, however if you wish to use it in your own server please get in contact with the developer, whose username is in the footer\nAlso, the source code is public on [github](https://github.com/MasterChief-John-117/GenericBot). You can also open bug reports on GitHub ")
                    .AddField($"Getting Started", $"See everything you can make me do with `{prefix}help`. {config}")
                    .AddField($"Self Assignable Roles", $"One of the most common public features GenericBot is used for is roles a user can assign to themself. To see all the avaible roles, do `{prefix}userroles`. You can join a role with `{prefix}iam [rolename]` or leave a role with `{prefix}iamnot [rolename]`.")
                    .AddField($"Moderation", $"GenericBot provides a wide range of tools for moderators to track users and infractions. It keeps track of all of a user's usernames, nicknames, and logged infractions, including kicks and timed or permanent bans. Users can be searched for either by ID, or by username or nickname, whether it be current or an old name. (All data is stored in an encrypted database, and data from one server is completely inaccessible by another server)")
                    .AddField($"Fun!", $"In addition to being a highly effective moderator toolkit, GenericBot has some fun commands, such as `{prefix}dog`, `{prefix}cat`, or `{prefix}jeff`. You can also create your own custom commands for rapid-fire memery or whatever else tickles your fancy");
                var embed = builder.Build();

                await msg.Channel.SendMessageAsync("", embed: embed);
            };

            botCommands.Add(info);

            Command confighelp = new Command("confighelp");
            confighelp.RequiredPermission = Command.PermissionLevels.Admin;
            confighelp.Description = "Show all the options to configure with syntax for each";
            confighelp.ToExecute += async (client, msg, parameters) =>
            {
                string prefix = GenericBot.GlobalConfiguration.DefaultPrefix;
                if (!String.IsNullOrEmpty(GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix))
                    prefix = GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix;

                var builder = new EmbedBuilder()
                    .WithTitle("GenericBot: Config Information")
                    .WithDescription("The `{prefix}config` command is huge and confusing. This aims to make it a bit simpler (For more general assistance, try `{prefix}info`)")
                    .WithUrl("https://github.com/MasterChief-John-117/GenericBot")
                    .WithColor(new Color(0xEF4347))
                    .WithFooter(footer =>
                    {
                        footer
                            .WithText($"If you have questions or notice any errors, please contact {GenericBot.DiscordClient.GetApplicationInfoAsync().Result.Owner.ToString()}");
                    })
                    .WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/1/10/Gear_1.svg/1000px-Gear_1.svg.png")
                    .AddField("AdminRoles", $"Add or remove Admin Roles by ID\nSyntax: `{prefix}config adminroles <add/remove> [roleId]`")
                    .AddField("ModeratorRoles (ModRoles)", $"Add or remove Moderator Roles by ID\nSyntax: `{prefix}config modroles <add/remove> [roleId]`")
                    .AddField("UserRoles", $"Add or remove User-Assignable Roles by ID\nSyntax: `{prefix}config userroles <add/remove> [roleId]`")
                    .AddField("Twitter", $"Enable or Disable tweeting from the server through the bot\nSyntax: `{prefix}config twitter <true/false>`")
                    .AddField("Prefix", $"Set the prefix to a given string. If [prefixString] is empty it gets set to the default of `{GenericBot.GlobalConfiguration.DefaultPrefix}`\nSyntax: `{prefix}config prefix [prefixString]`")
                    .AddField("Logging", $"Set the channel for logging by Id\nSyntax: `{prefix}config logging channelId [channelId]`\n\nToggle ignoring channels for logging by Id. Lists all ignored channels if channelId is empty\nSyntax`{prefix}config logging ignoreChannel [channelId]`")
                    .AddField("MutedRoleId", $"Set the role assigned by the `{prefix}mute` command. Set [roleId] to `0` to disable muting\nSyntax: `{prefix}config mutedRoleId [roleId]`")
                    .AddField("Verification", $"Get or Set the RoleId assigned for verification. Leave [roleId] empty to get the current role. Use `0` for the [roleId] to disable verification\nSyntax: `{prefix}config verification roleId [roleId]`\n\nGet or set the message sent for verification. Leave [message] empty to get the current message\nSyntax: `{prefix}config verification message [message]`")
                    .AddField("Points", $"Toggle whether points are enabled on the server\nSyntax: `{prefix}config points enabled`")
                    .AddField("GlobalBanOptOut", $"If a user has been proved to be engaging in illegal acts such as distributing underage porn, sometimes the bot owner will ban them from all servers the bot is in. You can opt out of this if you want\nSyntax: `{prefix}config globalbanoptout <true/false>`")
                    .AddField("AutoRole", $"Add or remove a role to be automatically granted by Id\nSyntax: `{prefix}config autorole <add/remove> [roleId]`");
                var embed = builder.Build();

                await msg.Channel.SendMessageAsync("", embed: embed);
            };

            botCommands.Add(confighelp);

            Command say = new Command("say");
            say.Delete = true;
            say.Aliases = new List<string>{"echo"};
            say.Description = "Say something a contributor said";
            say.SendTyping = true;
            say.Usage = "say <phrase>";
            say.ToExecute += async (client, msg, paramList) =>
            {
                if (!File.Exists("files/contributors.json"))
                {
                    File.WriteAllText("files/contributors.json", "{}");
                }
                var Contributors = JsonConvert.DeserializeObject<List<ulong>>(File.ReadAllText("files/contributors.json"));
                if (!(Contributors.Contains(msg.Author.Id) || say.GetPermissions(msg.Author, msg.GetGuild().Id) >= Command.PermissionLevels.GlobalAdmin))
                    return;

                ulong channelid = msg.Channel.Id;
                if (ulong.TryParse(paramList[0], out channelid))
                {
                    paramList.RemoveAt(0);
                    await ((ITextChannel) client.GetChannel(channelid)).SendMessageAsync(paramList.reJoin());
                    return;
                }
                else
                {
                    await msg.ReplyAsync(paramList.reJoin());
                }
            };

            botCommands.Add(say);

            Command dmuser = new Command("dmuser");
            dmuser.RequiredPermission = Command.PermissionLevels.BotOwner;
            dmuser.ToExecute += async (client, msg, paramList) =>
            {
                var channel = client.GetUser(ulong.Parse(paramList[0])).GetOrCreateDMChannelAsync().Result;

                if (paramList.Count == 1)
                {
                    var messages = channel.GetMessagesAsync().Flatten().Result.Reverse();
                    string str = messages.Select(m => $"{m.Author}: {m.Content} {m.Attachments.Select(a => a.Url).ToList().SumAnd()}").Aggregate((a, b) => a + "\n" + b);
                    File.WriteAllText("file.txt", str);
                    await msg.Channel.SendFileAsync("file.txt");
                    File.Delete("file.txt");
                    return;
                }
                string pref = GenericBot.GlobalConfiguration.DefaultPrefix;
                if (!String.IsNullOrEmpty(GenericBot.GuildConfigs[msg.GetGuild().Id].Prefix))
                    pref = GenericBot.GuildConfigs[msg.GetGuild().Id].Prefix;
                string message = msg.Content;
                message = message.Remove(0, pref.Length).TrimStart(' ').Remove(0, "dmUser".Length).TrimStart(' ')
                    .Remove(0, paramList[0].Length).TrimStart(' ');

                await channel.SendMessageAsync(message);
                await msg.ReplyAsync($"Sent `{message}` to {client.GetUser(ulong.Parse(paramList[0]))}");
            };

            botCommands.Add(dmuser);

            Command getGuild = new Command("getGuild");
            getGuild.Description = "Get a guild or list of guilds matching a pattern";
            getGuild.RequiredPermission = Command.PermissionLevels.BotOwner;
            getGuild.Aliases = new List<string>() { "getguilds" };
            getGuild.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    string reply = "";
                    foreach (var guild in client.Guilds)
                    {
                        reply += $"{guild.Name} (`{guild.Id}`)\n";
                    }

                    await msg.ReplyAsync(reply);
                }
                else if (ulong.TryParse(parameters[0], out ulong id))
                {
                    string reply = "";
                    foreach (var guild in client.Guilds)
                    {
                        if (guild.Id == id)
                            reply +=
                                $"Guild Match: {guild.Name} (`{guild.Id}`) Owner: {guild.Owner} (`{guild.Owner.Id}`)\n";
                        if (guild.OwnerId == id)
                            reply +=
                                $"Owner Match: {guild.Name} (`{guild.Id}`) Owner: {guild.Owner} (`{guild.Owner.Id}`)\n";
                    }

                    await msg.ReplyAsync(reply);
                }
            };

            botCommands.Add(getGuild);

            Command getInvite = new Command("getInvite");
            getInvite.Description = "Get a guild invite matching an ID";
            getInvite.RequiredPermission = Command.PermissionLevels.BotOwner;
            getInvite.ToExecute += async (client, msg, parameters) =>
            {
                if (!parameters.Empty() && ulong.TryParse(parameters[0], out ulong id))
                {
                    if (client.Guilds.HasElement(g => g.Id == id, out var guild))
                    {
                        await msg.ReplyAsync($"Guild: {guild.Name}\nOwner: {guild.Owner}\nInvite: {guild.DefaultChannel.CreateInviteAsync(maxUses: 1).Result.Url}");
                    }
                    else await msg.ReplyAsync("Guild does not exist");
                }
                else await msg.ReplyAsync("Invalid format");
            };
            botCommands.Add(getInvite);


            Command leaveGuild = new Command("leaveGuild");
            leaveGuild.Description = "Instruct the bot to leave the guild";
            leaveGuild.RequiredPermission = Command.PermissionLevels.Admin;
            leaveGuild.ToExecute += async (client, msg, parameters) =>
            {
                if (!parameters.Empty() &&
                    leaveGuild.GetPermissions(msg.Author, msg.GetGuild().Id) >= Command.PermissionLevels.GlobalAdmin)
                {
                    if (ulong.TryParse(parameters[0], out ulong guildId))
                    {
                        await client.GetGuild(guildId).LeaveAsync();
                        await msg.ReplyAsync("Done.");
                    }
                }
                else {
                    await msg.ReplyAsync("Bye!");
                    await msg.GetGuild().LeaveAsync();
                }
            };

            botCommands.Add(leaveGuild);

            Command tweet = new Command("tweet");
            tweet.Description = "Send a tweet from the @GenericBoTweets account";
            tweet.Usage = "tweet <message>";
            tweet.RequiredPermission = Command.PermissionLevels.User;
            tweet.ToExecute += async (client, msg, parameters) =>
            {
                if (!GenericBot.GuildConfigs[msg.GetGuild().Id].AllowTwitter && tweet.GetPermissions(msg.Author, msg.GetGuild().Id) < Command.PermissionLevels.GlobalAdmin)
                {
                    await msg.ReplyAsync("That command has been disabled on this guild");
                    return;
                }


                string message = parameters.reJoin();
                message = (msg.Author.Username + ": " + message);
                if (message.Length > 275)
                {
                    message = message.Substring(0, 275) + "...";
                }

                if (tweet.GetPermissions(msg.Author, msg.GetGuild().Id) < Command.PermissionLevels.GlobalAdmin)
                {
                    GenericBot.TweetQueue.AddLast(new QueuedTweet(msg, message));
                    await msg.ReplyAsync($"Your tweet has been added to the queue! It'll be sent in around `{GenericBot.TweetQueue.Count}` minutes");
                }
                else
                {
                    var response =  GenericBot.Twitter.SendTweetAsync(new SendTweetOptions
                    {
                        Status = message
                    }).Result;

                    if (response.Response.Error != null)
                    {
                        await msg.ReplyAsync($"{msg.Author.Mention}, there was an error sending your tweet: {response.Response.Error.Message}");
                        await GenericBot.Logger.LogErrorMessage(
                            $"{msg.Author.Id} tried tweeting {message}. Failure: {response.Response.Error.Message}");
                        GenericBot.TweetStore.Add(new GenericTweet(msg, message, null, false));
                    }
                    else
                    {
                        await msg.ReplyAsync($"{msg.Author.Mention}, your tweet is here: {response.Value.ToTwitterUrl()}");
                        await GenericBot.Logger.LogGenericMessage($"{msg.Author.Id} tweeted {response.Value.ToTwitterUrl()}");
                        GenericBot.TweetStore.Add(new GenericTweet(msg, message, response.Value.ToTwitterUrl().ToString(), true));
                    }
                    File.WriteAllText("files/tweetStore.json", JsonConvert.SerializeObject(GenericBot.TweetStore, Formatting.Indented));
                }
            };

            botCommands.Add(tweet);

            Command fourchannel = new Command("4channel");
            fourchannel.RequiredPermission = Command.PermissionLevels.Admin;
            fourchannel.ToExecute += async (client, msg, parameters) =>
            {
                var conf = GenericBot.GuildConfigs[msg.GetGuild().Id];
                if (conf.FourChannelId != msg.Channel.Id)
                {
                    conf.FourChannelId = msg.Channel.Id;
                    await msg.ReplyAsync("This channel has been set for 4chan-ification");
                }
                else
                {
                    conf.FourChannelId = 0;
                    await msg.ReplyAsync("4chan channel has been cleared");
                }
                conf.Save();
            };
            botCommands.Add(fourchannel);


            Command setStatus = new Command("setstatus");
            setStatus.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            setStatus.Description = "Set the playing status of the bot";
            setStatus.Usage = "setStatus <text>";
            setStatus.ToExecute += async (client, msg, parameters) =>
            {
                foreach (var shard in GenericBot.DiscordClient.Shards)
                {
                    await shard.SetGameAsync(parameters.reJoin());
                }

                await msg.ReplyAsync(
                    $"Set the status for `{GenericBot.DiscordClient.Shards.Count}` shards to `{parameters.reJoin()}`");

                GenericBot.GlobalConfiguration.PlayingStatus = parameters.reJoin();
                File.WriteAllText("files/config.json", JsonConvert.SerializeObject(GenericBot.GlobalConfiguration, Formatting.Indented));
            };

            botCommands.Add(setStatus);

            Command reportBug = new Command("reportbug");
            reportBug.Description = "Report a bug or suggest a feature for the bot";
            reportBug.Usage = "reportbug <bug>";
            reportBug.SendTyping = true;
            reportBug.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync("You have to include a bug report to send.");
                    return;
                }
                UserBugReport report = new UserBugReport();
                report.BugID = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                report.ReporterId = msg.Author.Id;
                report.Report = parameters.reJoin();

                string bugReport = "**Bug Report/Feature Suggestion**\n";
                bugReport += $"Sent By: `{msg.Author}`(`{msg.Author.Id}`)\n";
                bugReport += $"ID: `{report.BugID}`\n";
                bugReport += $"Report: ```\n{report.Report}\n```";

                await client.GetApplicationInfoAsync().Result.Owner.SendMessageAsync(bugReport);

                List<UserBugReport> bugs =
                    JsonConvert.DeserializeObject<List<UserBugReport>>(File.ReadAllText("files/bugReports.json"));

                bugs.Add(report);

                File.WriteAllText("files/bugReports.json", JsonConvert.SerializeObject(bugs, Formatting.Indented));

                await msg.ReplyAsync("Your bug report has been sucessfully recieved!");
            };

            botCommands.Add(reportBug);

            Command closeBug = new Command("closebug");
            closeBug.Description = "Close a bug report or feature request";
            closeBug.RequiredPermission = Command.PermissionLevels.BotOwner;
            closeBug.ToExecute += async (client, msg, parameters) =>
            {
                List<UserBugReport> bugs =
                    JsonConvert.DeserializeObject<List<UserBugReport>>(File.ReadAllText("files/bugReports.json"));

                UserBugReport bug;
                if (bugs.HasElement(b => b.BugID == parameters[0], out bug))
                {
                    bug.IsOpen = false;
                    var user = client.GetUser(bug.ReporterId);
                    parameters.RemoveAt(0);
                    bug.Repsonse = parameters.reJoin();
                    bug.ClosedAt = DateTimeOffset.UtcNow;
                    try
                    {
                        await user.SendMessageAsync(
                            $"Hello! Your bug report/feature suggestion ({bug.Report.SafeSubstring(80)}) has been closed with the following message: {parameters.reJoin()}");
                    }
                    catch (Exception ex)
                    {
                        await msg.ReplyAsync($"Could not message to `{user}`(`{user.Id}`)");
                    }
                    File.WriteAllText("files/bugReports.json", JsonConvert.SerializeObject(bugs, Formatting.Indented));

                    await msg.Channel.SendMessageAsync("Done.");
                }
                else
                {
                    await msg.ReplyAsync("That's not a bug");
                }
            };

            botCommands.Add(closeBug);

            Command getBugs = new Command("getBugs");
            getBugs.Aliases = new List<string>{"getbug"};
            getBugs.Description = "Get all open bugs";
            getBugs.RequiredPermission = Command.PermissionLevels.BotOwner;
            getBugs.ToExecute += async (client, msg, parameters) =>
            {
                List<UserBugReport> bugs =
                    JsonConvert.DeserializeObject<List<UserBugReport>>(File.ReadAllText("files/bugReports.json"));

                if (parameters.Empty())
                {
                    string openBugs = $"**There are `{bugs.Count(b => b.IsOpen)}` open bugs**\n\n";
                    foreach (var bug in bugs.Where(b => b.IsOpen))
                    {
                        openBugs += $"Sent By: `{client.GetUser(bug.ReporterId)}`(`{bug.ReporterId}`)\n";
                        openBugs += $"ID: `{bug.BugID}`\n";
                        openBugs +=
                            $"Time: `{DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(bug.BugID)).ToLocalTime()}GMT`\n";
                        openBugs += $"Report: ```\n{bug.Report}\n```\n\n";
                    }

                    foreach (var str in openBugs.SplitSafe('\n'))
                    {
                        await msg.ReplyAsync(str);
                    }
                }
                else
                {
                    var bug = bugs.First(b => b.BugID == parameters[0]);
                    string openBugs = "";
                    openBugs += $"Sent By: `{client.GetUser(bug.ReporterId)}`(`{bug.ReporterId}`)\n";
                    openBugs += $"ID: `{bug.BugID}`\n";
                    openBugs +=
                        $"Opened At: `{DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(bug.BugID)).ToLocalTime()}GMT`\n";
                    openBugs += $"Closed At: `{bug.ClosedAt.ToLocalTime()}GMT`\n";
                    openBugs += $"Report: ```\n{bug.Report}\n```";
                    openBugs += $"Response: ```\n{bug.Repsonse}\n```";

                    await msg.ReplyAsync(openBugs);
                }
            };

            botCommands.Add(getBugs);

            Command blacklist = new Command("blacklist");
            blacklist.Description = "Add or remove someone from the blacklist";
            blacklist.Usage = "blacklist [add|remove] [id]";
            blacklist.RequiredPermission = Command.PermissionLevels.BotOwner;
            blacklist.ToExecute += async (client, msg, parameters) =>
            {
                ulong id;
                if (parameters[0].ToLower().Equals("add"))
                {
                    if (ulong.TryParse(parameters[1], out id))
                    {
                        if (!GenericBot.GlobalConfiguration.BlacklistedIds.Contains(id))
                        {
                            GenericBot.GlobalConfiguration.BlacklistedIds.Add(id);
                            await msg.ReplyAsync($"Blacklisted `{id}`");
                            GenericBot.GlobalConfiguration.Save();
                            return;
                        }
                        else
                        {
                            await msg.ReplyAsync($"`{id}` is already blacklisted");
                            return;
                        }
                    }
                    else
                    {
                        await msg.ReplyAsync("Invalid ID");
                        return;
                    }
                }
                else if (parameters[0].ToLower().Equals("remove"))
                {
                    if (ulong.TryParse(parameters[1], out id))
                    {
                        if (GenericBot.GlobalConfiguration.BlacklistedIds.Contains(id))
                        {
                            GenericBot.GlobalConfiguration.BlacklistedIds.Remove(id);
                            await msg.ReplyAsync($"Un-Blacklisted `{id}`");
                            GenericBot.GlobalConfiguration.Save();
                            return;
                        }
                        else
                        {
                            await msg.ReplyAsync($"`{id}` is not blacklisted");
                            return;
                        }
                    }
                    else
                    {
                        await msg.ReplyAsync("Invalid ID");
                        return;
                    }
                }
                else
                {
                    await msg.ReplyAsync("Invalid Option");
                }
            };

            botCommands.Add(blacklist);

            return botCommands;
        }
    }
}
