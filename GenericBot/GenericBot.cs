using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TweetSharp;
using Timer = System.Timers.Timer;

namespace GenericBot
{
    class GenericBot
    {
        public static DiscordShardedClient DiscordClient;
        public static Logger Logger;
        public static GlobalConfiguration GlobalConfiguration;
        public static Dictionary<ulong, GuildConfig> GuildConfigs;
        public static List<Command> Commands = new List<Command>();
        public static string SessionId;
        public static bool DebugMode = false;

        public static TwitterService Twitter = new TwitterService("AfaD74ulbQQmjb1yDuGKWtVY9", "WAuRJS6Z4RUDgignHmsDzudbIx2YP4PgnAcz3tp7G7nd1ZHs2z");
        public static List<GenericTweet> TweetStore;
        public static LinkedList<QueuedTweet> TweetQueue = new LinkedList<QueuedTweet>();
        public static Timer TweetSender = new Timer();
        public static Timer Updater = new Timer();

        public static Dictionary<ulong, List<IMessage>> MessageDeleteQueue = new Dictionary<ulong, List<IMessage>>();
        public static Timer MessageDeleteTimer = new Timer();
        public static bool Test = true;

        static void Main(string[] args)
        {
            Logger = new Logger(GetStringSha256Hash(DateTime.UtcNow.ToString()));
            GlobalConfiguration = new GlobalConfiguration().Load();
            GuildConfigs = new Dictionary<ulong, GuildConfig>();
            TweetStore = JsonConvert.DeserializeObject<List<GenericTweet>>(File.ReadAllText("files/tweetStore.json"));

            Twitter.AuthenticateWith("924464831813398529-pi51h6UB3iitJB2UQwGrHukYjD1Pz7F", "3R0vFFQLCGe9vuGvn00Avduq1K8NHjmRBUFJVuo9nRYXJ");

            TweetSender.AutoReset = true;
            TweetSender.Interval = 60 * 1000;
            TweetSender.Elapsed += TweetSenderOnElapsed;
            TweetSender.Start();

            Updater.AutoReset = true;
            Updater.Interval = 5 * 1000;
            Updater.Elapsed += CheckMuteRemoval;

            MessageDeleteTimer.AutoReset = true;
            MessageDeleteTimer.Interval = 60 * 1000;
            MessageDeleteTimer.Elapsed += MessageDeleteTimerOnElapsed;
            MessageDeleteTimer.Start();

            new GenericBot().Start().GetAwaiter().GetResult();
        }
        public async Task Start()
        {
            DiscordClient = new DiscordShardedClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100,
            });

            DiscordClient.Log += Logger.LogClientMessage;

            try
            {
                await DiscordClient.LoginAsync(TokenType.Bot, GlobalConfiguration.Token);
                await DiscordClient.StartAsync();
            }
            catch (Exception e)
            {
                await Logger.LogErrorMessage($"{e.Message}\n{e.StackTrace.Split('\n').First(s => s.Contains("line"))}");
            }

            if (GlobalConfiguration.OwnerId == 0)
            {
                GlobalConfiguration.OwnerId = DiscordClient.GetApplicationInfoAsync().Result.Owner.Id;
                GlobalConfiguration.Save();
            }

            foreach (var shard in DiscordClient.Shards)
            {
                shard.Ready += OnReady;
                shard.SetGameAsync(">help | Everything go boom").FireAndForget();
            }

            var serviceProvider = ConfigureServices();

            var _handler = new CommandHandler();
            await _handler.Install(serviceProvider);

            Updater.Start();

            // Block this program until it is closed.
            await Task.Delay(-1);
        }

        private async Task OnReady()
        {
            foreach (var guild in DiscordClient.Guilds)
            {
                GuildConfigs.Add(guild.Id, JsonConvert.DeserializeObject<GuildConfig>(File.ReadAllText($"files/guildConfigs/{guild.Id}.json")));
            }
            foreach (var guild in DiscordClient.Guilds.Where(g => !GuildConfigs.Keys.Contains(g.Id)))
            {
                GuildConfigs.Add(guild.Id, new GuildConfig(guild.Id).Save());
            }
            await Logger.LogGenericMessage($"Loaded {GuildConfigs.Count} Configs on Startup");
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(DiscordClient);
            var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
            return provider;
        }


        private static void MessageDeleteTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            foreach (var kvp in MessageDeleteQueue)
            {
                ((ISocketMessageChannel) DiscordClient.GetChannel(kvp.Key)).DeleteMessagesAsync(kvp.Value);
                MessageDeleteQueue.Remove(kvp.Key);
            }
        }

        private static void CheckMuteRemoval(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            foreach (var mute in GuildConfigs.SelectMany(g => g.Value.ChannelMutes))
            {
                if (mute.RemovealTime < DateTime.UtcNow)
                {
                    (DiscordClient.GetChannel(mute.ChannelId) as SocketTextChannel)
                        .RemovePermissionOverwriteAsync(DiscordClient.GetUser(mute.UserId));
                    GuildConfigs[((SocketGuildChannel) DiscordClient.GetChannel(mute.ChannelId)).Guild.Id].ChannelMutes
                        .Remove(mute);
                    GenericBot.Logger.LogGenericMessage($"Unmuted {mute.UserId} from {mute.ChannelId}");
                }
            }
            File.WriteAllText("files/guildConfigs.json", JsonConvert.SerializeObject(GuildConfigs, Formatting.Indented));
        }

        private static async void TweetSenderOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (!TweetQueue.Any())
            {
                return;
            }

            var tweetInfo = TweetQueue.First.Value;
            var msg = tweetInfo.msg;

            var response =  GenericBot.Twitter.SendTweetAsync(new SendTweetOptions
            {
                Status = tweetInfo.InputMessage
            }).Result;

            if (response.Response.Error != null)
            {
                await msg.ReplyAsync($"{msg.Author.Mention}, there was an error sending your tweet: {response.Response.Error.Message}");
                await GenericBot.Logger.LogErrorMessage(
                    $"{msg.Author.Id} tried tweeting {tweetInfo.InputMessage}. Failure: {response.Response.Error.Message}");
                GenericBot.TweetStore.Add(new GenericTweet(msg, tweetInfo.InputMessage, null, false));
            }
            else
            {
                await msg.ReplyAsync($"{msg.Author.Mention}, your tweet is here: {response.Value.ToTwitterUrl()}");
                await GenericBot.Logger.LogGenericMessage($"{msg.Author.Id} tweeted {response.Value.ToTwitterUrl()}");
                GenericBot.TweetStore.Add(new GenericTweet(msg, tweetInfo.InputMessage, response.Value.ToTwitterUrl().ToString(), true));
            }

            TweetQueue.RemoveFirst();
            File.WriteAllText("files/tweetStore.json", JsonConvert.SerializeObject(GenericBot.TweetStore, Formatting.Indented));

        }

        internal static string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
            }
        }

        public static void QueueMessagesForDelete(List<IMessage> messages)
        {
            try
            {
                GenericBot.MessageDeleteQueue.Add(messages.First().Channel.Id, messages);
            }
            catch (Exception ex)
            {
                GenericBot.MessageDeleteQueue[messages.First().Channel.Id].AddRange(messages);
            }
        }
    }
}
