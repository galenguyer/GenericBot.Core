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
                               $"SessionID: `{GenericBot.Logger.SessionId}`\n\n" +
                               $"Servers: `{client.Guilds.Count}`\n" +
                               $"Users: `{client.Guilds.Sum(g => g.Users.Count)}`\n" +
                               $"Shards: `{client.Shards.Count}`\n" +
                               $"Memory: `{Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB`\n" +
                               $"Threads: `{Process.GetCurrentProcess().Threads.OfType<ProcessThread>().Count()}` " +
                               $"(`{Process.GetCurrentProcess().Threads.OfType<ProcessThread>().Count(t => t.ThreadState == ThreadState.Running)} Active`)\n" +
                               $"Uptime: `{(DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss")}`\n" +
                               $"Disconnects: `{GenericBot.Disconnects}`\n\n";

                foreach (var shard in GenericBot.DiscordClient.Shards)
                {
                    stats += $"Shard `{shard.ShardId}`: `{shard.Guilds.Count}` Guilds (`{shard.Guilds.Sum(g => g.Users.Count)}` Users)\n";
                }
                               await msg.Channel.SendMessageAsync(stats);
            };
            botCommands.Add(global);

            Command say = new Command("say");
            say.Delete = true;
            say.Aliases = new List<string>{"echo"};
            say.Description = "Say something a contributor said";
            say.SendTyping = true;
            say.Usage = "say <phrase>";
            say.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            say.ToExecute += async (client, msg, paramList) =>
            {
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

            Command leaveGuild = new Command("leaveGuild");
            leaveGuild.Description = "Instruct the bot to leave the guild";
            leaveGuild.RequiredPermission = Command.PermissionLevels.Admin;
            leaveGuild.ToExecute += async (client, msg, parameters) =>
            {
                if (!parameters.Empty() &&
                    leaveGuild.GetPermissions(msg.Author, msg.GetGuild().Id) >= Command.PermissionLevels.GlobalAdmin)
                {
                    ulong guildId;
                    if (ulong.TryParse(parameters[0], out guildId))
                    {
                        await msg.GetGuild().LeaveAsync();
                        await msg.ReplyAsync("Done.");
                    }
                }
                {
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
                if (message.Length > 135)
                {
                    message = message.Substring(0, 135) + "...";
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
