using Discord;
using Discord.WebSocket;
using GenericBot.CommandModules;
using GenericBot.Database;
using GenericBot.Entities;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GenericBot
{
    public static class Core
    {
        public static GlobalConfiguration GlobalConfig { get; private set; }
        public static DiscordShardedClient DiscordClient { get; private set; }
        public static List<Command> Commands { get; set; }
        public static Dictionary<ulong, List<CustomCommand>> CustomCommands;
        public static int Messages { get; set; }
        public static Logger Logger { get; private set; }
        private static MongoEngine MongoEngine { get; set; }

        //private static List<GuildConfig> LoadedGuildConfigs;

        static Core()
        {
            // Load global configs
            GlobalConfig = new GlobalConfiguration().Load();
            Logger = new Logger();
            Commands = new List<Command>();
            CustomCommands = new Dictionary<ulong, List<CustomCommand>>();
            LoadCommands(GlobalConfig.CommandsToExclude);
            MongoEngine = new MongoEngine();
            LoadedGuildConfigs = new List<GuildConfig>();
            Messages = 0;

            // Configure Client
            DiscordClient = new DiscordShardedClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100,
            });
            DiscordClient.Log += Logger.LogClientMessage;
            DiscordClient.MessageReceived += MessageEventHandler.MessageRecieved;
            DiscordClient.MessageUpdated += MessageEventHandler.HandleEditedCommand;
            DiscordClient.MessageDeleted += MessageEventHandler.MessageDeleted;
            DiscordClient.UserJoined += UserEventHandler.UserJoined;
            DiscordClient.UserLeft += UserEventHandler.UserLeft;
            DiscordClient.UserUpdated += UserEventHandler.UserUpdated;
            DiscordClient.GuildAvailable += GuildEventHandler.GuildLoaded;
            DiscordClient.ShardReady += ShardReady;
        }

        private static async Task ShardReady(DiscordSocketClient arg)
        {
            if (File.Exists("version.txt"))
            {
                await arg.SetGameAsync($"v. {File.ReadAllText("version.txt")}", type: ActivityType.Watching);
            }
            else
            {
                await arg.SetGameAsync(GlobalConfig.PlayingStatus);
            }
        }

        private static void LoadCommands(List<string> CommandsToExclude = null)
        {
            Commands.Clear();
            Commands.AddRange(new InfoModule().Load());
            Commands.AddRange(new ConfigModule().Load());
            Commands.AddRange(new RoleModule().Load());
            Commands.AddRange(new MemeModule().Load());
            Commands.AddRange(new CustomCommandModule().Load());
            Commands.AddRange(new BanModule().Load());
            Commands.AddRange(new QuoteModule().Load());
            Commands.AddRange(new WarningModule().Load());
            Commands.AddRange(new LookupModule().Load());
            Commands.AddRange(new MuteModule().Load());
            Commands.AddRange(new ClearModule().Load());
            Commands.AddRange(new SocialModule().Load());
            Commands.AddRange(new QuickCommands().GetQuickCommands());
            Commands.AddRange(new GetGuildModule().Load());
            Commands.AddRange(new GiveawayModule().Load());
            Commands.AddRange(new ImageModule().Load());

            if (CommandsToExclude == null)
                return;
            Commands = Commands.Where(c => !CommandsToExclude.Contains(c.Name)).ToList();
        }

        public static bool CheckBlacklisted(ulong UserId) => GlobalConfig.BlacklistedIds.Contains(UserId);
        public static ulong GetCurrentUserId() => DiscordClient.CurrentUser.Id;
        public static ulong GetOwnerId() => DiscordClient.GetApplicationInfoAsync().Result.Owner.Id;
        public static string GetGlobalPrefix() => GlobalConfig.DefaultPrefix;
        public static string GetPrefix(ParsedCommand context)
        {
            if (!(context.Message.Channel is SocketDMChannel) && (context.Guild == null || !string.IsNullOrEmpty(GetGuildConfig(context.Guild.Id).Prefix)))
                return GetGuildConfig(context.Guild.Id).Prefix;
            return GetGlobalPrefix();
        }
        public static bool CheckGlobalAdmin(ulong UserId) => GlobalConfig.GlobalAdminIds.Contains(UserId);
        public static SocketGuild GetGuid(ulong GuildId) => DiscordClient.GetGuild(GuildId);

        public static GuildConfig GetGuildConfig(ulong GuildId)
        {
            //if (LoadedGuildConfigs.Any(c => c.Id == GuildId))
            //{
            //    return LoadedGuildConfigs.Find(c => c.Id == GuildId);
            //}
            //else
            //{
            //    LoadedGuildConfigs.Add(MongoEngine.GetGuildConfig(GuildId));
            //}
            return GetGuildConfig(GuildId); 
        }
        public static GuildConfig SaveGuildConfig(GuildConfig guildConfig)
        {
            //if (LoadedGuildConfigs.Any(c => c.Id == guildConfig.Id))
            //    LoadedGuildConfigs.RemoveAll(c => c.Id == guildConfig.Id);
            //LoadedGuildConfigs.Add(guildConfig);

            return MongoEngine.SaveGuildConfig(guildConfig);
        }
        public static List<CustomCommand> GetCustomCommands(ulong guildId)
        {
            if (CustomCommands.ContainsKey(guildId))
                return CustomCommands[guildId];
            else
            {
                var cmds = MongoEngine.GetCustomCommands(guildId);
                CustomCommands.Add(guildId, cmds);
                return cmds;
            }
        }
        public static CustomCommand SaveCustomCommand(CustomCommand command, ulong guildId)
        {
            if (CustomCommands.ContainsKey(guildId))
            {
                if (CustomCommands[guildId].Any(c => c.Name == command.Name))
                {
                    CustomCommands[guildId].RemoveAll(c => c.Name == command.Name);
                }
                CustomCommands[guildId].Add(command);
            }
            else
            {
                CustomCommands.Add(guildId, new List<CustomCommand> { command });
            }
            MongoEngine.SaveCustomCommand(command, guildId);
            return command;
        }

        public static void DeleteCustomCommand(string name, ulong guildId)
        {
            if (CustomCommands.ContainsKey(guildId))
            {
                CustomCommands[guildId].RemoveAll(c => c.Name == name);
            }
            MongoEngine.DeleteCustomCommand(name, guildId);
        }

        public static Quote AddQuote(string quote, ulong guildId) =>
            MongoEngine.AddQuote(quote, guildId);
        public static bool RemoveQuote(int id, ulong guildId) =>
            MongoEngine.RemoveQuote(id, guildId);

        public static Quote GetQuote(string quote, ulong guildId)
        {
            var quotes = MongoEngine.GetAllQuotes(guildId);

            if (string.IsNullOrEmpty(quote))
            {
                return quotes.GetRandomItem();
            }
            else if (int.TryParse(quote, out int id))
            {
                return quotes.Any(q => q.Id == id) ? quotes.Find(q => q.Id == id) : new Quote("Not Found", 0);
            }
            else
            {
                var foundQuotes = quotes.Where(q => q.Content.ToLower().Contains(quote.ToLower())).ToList();
                return foundQuotes.Any() ? foundQuotes.GetRandomItem() : new Quote("Not Found", 0);
            }
        }

        public static DatabaseUser SaveUserToGuild(DatabaseUser user, ulong guildId, bool log = true) =>
            MongoEngine.SaveUserToGuild(user, guildId, log);
        public static DatabaseUser GetUserFromGuild(ulong userId, ulong guildId, bool log = true) =>
            MongoEngine.GetUserFromGuild(userId, guildId, log);
        public static List<DatabaseUser> GetAllUsers(ulong guildId) =>
            MongoEngine.GetAllUsers(guildId);

        public static GenericBan SaveBanToGuild(GenericBan ban, ulong guildId) =>
            MongoEngine.SaveBanToGuild(ban, guildId);
        public static List<GenericBan> GetBansFromGuild(ulong guildId, bool log = true) =>
            MongoEngine.GetBansFromGuild(guildId, log);
        public static void RemoveBanFromGuild(ulong banId, ulong guildId) =>
            MongoEngine.RemoveBanFromGuild(banId, guildId);

        public static void AddToAuditLog(ParsedCommand command, ulong guildId) =>
            MongoEngine.AddToAuditLog(command, guildId);
        public static List<AuditCommand> GetAuditLog(ulong guildId) =>
            MongoEngine.GetAuditLog(guildId);

        public static void AddStatus(Status status) =>
            MongoEngine.AddStatus(status);

        public static Giveaway CreateGiveaway(Giveaway giveaway, ulong guildId) =>
            MongoEngine.CreateGiveaway(giveaway, guildId);

        public static Giveaway UpdateOrCreateGiveaway(Giveaway giveaway, ulong guildId) =>
            MongoEngine.UpdateOrCreateGiveaway(giveaway, guildId);

        public static List<Giveaway> GetGiveaways(ulong guildId) =>
            MongoEngine.GetGiveaways(guildId);

        public static void DeleteGiveaway(Giveaway giveaway, ulong guildId) =>
            MongoEngine.DeleteGiveaway(giveaway, guildId);

        public static ExceptionReport AddOrUpdateExceptionReport(ExceptionReport report)
        {
            report = MongoEngine.AddOrUpdateExceptionReport(report);

            if(!report.Reported && report.Count >= 5 && !string.IsNullOrEmpty(GlobalConfig.GithubToken))
            {
                try
                {
                    var githubTokenAuth = new Credentials(GlobalConfig.GithubToken);
                    var client = new GitHubClient(new ProductHeaderValue("GenericBot"));
                    client.Credentials = githubTokenAuth;
                    var issueToCreate = new NewIssue($"AUTOMATED: {report.Message}");
                    issueToCreate.Body = $"Stacktrace:\n" +
                        $"{report.StackTrace}\n" +
                        $"\n" +
                        $"Reporting Build (if available): {GenericBot.BuildId}\n";

                    var issue = client.Issue.Create(client.User.Current().Result.Login, "GenericBot", issueToCreate).Result;
                    report.Reported = true;
                }
                catch(Exception ex)
                {
                    Logger.LogGenericMessage("An error occured reporting to github. Please check your credentials and that there is a repo \"GenericBot\" associated with your account.");
                    Logger.LogGenericMessage(ex.Message + "\n" + ex.StackTrace);
                }
            }

            return report;
        }
    }
}
