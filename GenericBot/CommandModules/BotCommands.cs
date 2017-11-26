using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
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
            ping.RequiredPermission = Command.PermissionLevels.User;
            ping.ToExecute += async (client, msg, paramList) =>
            {
                var stop = new Stopwatch();
                stop.Start();
                var rep = await msg.Channel.SendMessageAsync("Pong!");

                if (paramList.FirstOrDefault() != null && paramList.FirstOrDefault().Equals("verbose"))
                {
                    stop.Stop();
                    await rep.ModifyAsync(m => m.Content = $"Pong! `{stop.ElapsedMilliseconds}ms`");
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
                               $"Uptime: `{(DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss")}\n`";
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
                await msg.Channel.SendMessageAsync(paramList.Aggregate((i, j) => i + " " + j));
            };

            botCommands.Add(say);

            Command reload = new Command("reload");
            reload.RequiredPermission = Command.PermissionLevels.BotOwner;
            reload.Description = "Reload all configurations";
            reload.ToExecute += async (client, msg, parameters) =>
            {
                GenericBot.GlobalConfiguration =
                    JsonConvert.DeserializeObject<GlobalConfiguration>(File.ReadAllText("files/config.json"));
                await msg.ReplyAsync("Done!");
            };

            botCommands.Add(reload);

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
                    await msg.ReplyAsync($"Your tweet has been aded to the queue! It'll be sent in around `{GenericBot.TweetQueue.Count}` minutes");
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
            };

            botCommands.Add(setStatus);

            return botCommands;
        }
    }
}
