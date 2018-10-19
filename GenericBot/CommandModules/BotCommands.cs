using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using Newtonsoft.Json;
using TweetSharp;

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
                    var messages = channel.GetMessagesAsync().Flatten().ToEnumerable().Reverse().ToList();
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
            reportBug.Usage = "reportbug";
            reportBug.SendTyping = true;
            reportBug.ToExecute += async (client, msg, parameters) =>
            {
                await msg.ReplyAsync($"GenericBot no longer tracks bugs locally. These are now handled on GitHub with the issue " +
                    $"tracking system. You can find all open bugs and features here: https://github.com/MasterChief-John-117/GenericBot/issues." +
                    $" Feel free to open an issue if there's something you want to see. If you don't have a GitHub account, " +
                    $"you can DM the bot your suggestion and it will be added to the issue list within a week");
            };

            botCommands.Add(reportBug);

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

            Command notifyGlobal = new Command("notifyGlobal");
            notifyGlobal.RequiredPermission = Command.PermissionLevels.BotOwner;
            notifyGlobal.Description = "Notify all servers with the bot of something very very important";
            notifyGlobal.ToExecute += async (client, msg, parameters) =>
            {
                string message = msg.Content;
                string pref = GenericBot.GlobalConfiguration.DefaultPrefix;
                if (!String.IsNullOrEmpty(GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix))
                    pref = GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix;
                
                string param = message.Substring(pref.Length).Substring(message.Split(' ')[0].Length);

                int servers = 0;
                int owners = 0;
                foreach (var guild in client.Guilds)
                {
                    if (guild.TextChannels.HasElement(c => c.Id == GenericBot.GuildConfigs[guild.Id].UserLogChannelId, out var channel))
                    {
                        string notif = $"Hello, valued {guild.GetUser(client.CurrentUser.Id).GetDisplayName()}:tm: user! " +
                        $"A high-priority, global notification has been sent out to all servers using me:\n\n{param}";
                        await channel.SendMessageAsync(notif);
                        GenericBot.Logger.LogGenericMessage($"Sent {notif} to {guild.Name}");
                        servers++;
                    }
                    else
                    {
                        string notif = $"Hello, valued {guild.GetUser(client.CurrentUser.Id).GetDisplayName()}:tm: user! " +
                        $"A high-priority, global notification has been sent out to all servers using me (Because I couldn't " +
                        $"find a logging channel on {guild.Name}, I've sent it to you directly):\n\n{param}";
                        await guild.Owner.GetOrCreateDMChannelAsync().Result.SendMessageAsync(notif);
                        GenericBot.Logger.LogGenericMessage($"Sent {notif} to {guild.Name}'s owner, {guild.Owner.GetDisplayName()}");
                        owners++;
                    }
                }

                await msg.ReplyAsync($"Succesfully notified {servers} servers and {owners} owners!");
            };

            botCommands.Add(notifyGlobal);

            return botCommands;
        }
    }
}
