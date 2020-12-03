﻿using Discord;
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
    /// <summary>
    /// The Core client, responsible for loading everything on startup and wrapping
    /// any methods that interact with the database or configuration
    /// </summary>
    public static class Core
    {
        /// <summary>
        /// The shared configuration for the entire bot
        /// </summary>
        public static GlobalConfiguration GlobalConfig { get; private set; }
        public static DiscordShardedClient DiscordClient { get; private set; }
        public static List<Command> Commands { get; set; }
        public static Dictionary<ulong, List<CustomCommand>> CustomCommands;
        public static int Messages { get; set; }
        public static Logger Logger { get; private set; }
        private static IDatabaseEngine DatabaseEngine { get; set; }

        private static List<GuildConfig> LoadedGuildConfigs;

        static Core()
        {
            // Load the global configuration
            GlobalConfig = new GlobalConfiguration().Load();
            // Initialize a new logger with the current data and time
            Logger = new Logger();
            // Intialize a new, empty list of commands and custom commands
            Commands = new List<Command>();
            // CustomCommands are structured as GuildID -> List<CustomCommand>
            CustomCommands = new Dictionary<ulong, List<CustomCommand>>();
            // Load custom commands from enabled modules
            LoadCommands(GlobalConfig.CommandsToExclude);
            // Create the database engine
            DatabaseEngine = new MongoEngine();
            LoadedGuildConfigs = new List<GuildConfig>();
            Messages = 0;

            // Configure Client
            DiscordClient = new DiscordShardedClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100,
            });
            // Set up event handlers
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

        /// <summary>
        /// Set the playing message on each sharded instance once the bot reports it as Ready
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Load all enabled modules into the Commands list
        /// </summary>
        /// <param name="CommandsToExclude"></param>
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

        /// <summary>
        /// Check if a user is blacklisted from running bot commands globally
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public static bool CheckBlacklisted(ulong UserId) => 
            GlobalConfig.BlacklistedIds != null && GlobalConfig.BlacklistedIds.Contains(UserId);
        /// <summary>
        /// Return the UserID of the bot
        /// </summary>
        /// <returns></returns>
        public static ulong GetCurrentUserId() => DiscordClient.CurrentUser.Id;
        /// <summary>
        /// Return the UserID of the owner of the bot account
        /// </summary>
        /// <returns></returns>
        public static ulong GetOwnerId() => DiscordClient.GetApplicationInfoAsync().Result.Owner.Id;
        public static string GetGlobalPrefix() => GlobalConfig.DefaultPrefix;
        /// <summary>
        /// Return the appropriate prefix for a command, based on where the comand was run
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetPrefix(ParsedCommand context)
        {
            // TODO: This if check seems weird, the second statement looks very wrong
            if (!(context.Message.Channel is SocketDMChannel) 
                && context.Guild != null 
                && !string.IsNullOrEmpty(GetGuildConfig(context.Guild.Id).Prefix))
                return GetGuildConfig(context.Guild.Id).Prefix;
            return GetGlobalPrefix();
        }
        public static bool CheckGlobalAdmin(ulong UserId) => GlobalConfig.GlobalAdminIds.Contains(UserId);
        // TODO: Fix name, whoops
        public static SocketGuild GetGuid(ulong GuildId) => DiscordClient.GetGuild(GuildId);

        // TODO: Caching for the next two methods (GetGuildConfig and SaveGuildConfig) looks inconsistent and may cause issues
        /// <summary>
        /// Get the config for a guild by ID
        /// </summary>
        /// <param name="GuildId"></param>
        /// <returns></returns>
        public static GuildConfig GetGuildConfig(ulong GuildId)
        {
            if (LoadedGuildConfigs.Any(c => c.Id == GuildId))
            {
                return LoadedGuildConfigs.Find(c => c.Id == GuildId);
            }
            else
            {
                LoadedGuildConfigs.Add(DatabaseEngine.GetGuildConfig(GuildId));
            }
            return DatabaseEngine.GetGuildConfig(GuildId);
        }
        /// <summary>
        /// Write a GuildConfig to the database
        /// </summary>
        /// <param name="guildConfig"></param>
        /// <returns></returns>
        public static GuildConfig SaveGuildConfig(GuildConfig guildConfig)
        {
            //if (LoadedGuildConfigs.Any(c => c.Id == guildConfig.Id))
            //    LoadedGuildConfigs.RemoveAll(c => c.Id == guildConfig.Id);
            //LoadedGuildConfigs.Add(guildConfig);

            return DatabaseEngine.SaveGuildConfig(guildConfig);
        }

        /// <summary>
        /// Retrieve a list of custom commands for a guild
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public static List<CustomCommand> GetCustomCommands(ulong guildId)
        {
            if (CustomCommands.ContainsKey(guildId))
                return CustomCommands[guildId];
            else
            {
                var cmds = DatabaseEngine.GetCustomCommands(guildId);
                CustomCommands.Add(guildId, cmds);
                return cmds;
            }
        }

        // TODO: Ensure database/cache consistency for custom commands
        /// <summary>
        /// Add or overrwite a custom command to the cache and database
        /// </summary>
        /// <param name="command"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
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
            DatabaseEngine.SaveCustomCommand(command, guildId);
            return command;
        }

        /// <summary>
        /// Delete a custom command from the database by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guildId"></param>
        public static void DeleteCustomCommand(string name, ulong guildId)
        {
            if (CustomCommands.ContainsKey(guildId))
            {
                CustomCommands[guildId].RemoveAll(c => c.Name == name);
            }
            DatabaseEngine.DeleteCustomCommand(name, guildId);
        }

        public static Quote AddQuote(string quote, ulong guildId) =>
            DatabaseEngine.AddQuote(quote, guildId);
        public static bool RemoveQuote(int id, ulong guildId) =>
            DatabaseEngine.RemoveQuote(id, guildId);

        /// <summary>
        /// Retrieve a quote from the database. 
        /// If no string is provided, a random quote is returned.
        /// If a number is provided, the quote with that ID is attempted to be returned.
        /// Otherwise, a plaintext search is performed and a random matching quote is returned.
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public static Quote GetQuote(string quote, ulong guildId)
        {
            var quotes = GetAllQuotes(guildId);

            if (string.IsNullOrEmpty(quote))
            {
                return quotes.GetRandomItem();
            }
            else if (int.TryParse(quote, out int id) && id <= quotes.Max(q => q.Id))
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
            DatabaseEngine.SaveUserToGuild(user, guildId, log);
        public static DatabaseUser GetUserFromGuild(ulong userId, ulong guildId, bool log = true) =>
            DatabaseEngine.GetUserFromGuild(userId, guildId, log);
        public static List<DatabaseUser> GetAllUsers(ulong guildId) =>
            DatabaseEngine.GetAllUsers(guildId);

        public static GenericBan SaveBanToGuild(GenericBan ban, ulong guildId) =>
            DatabaseEngine.SaveBanToGuild(ban, guildId);
        public static List<GenericBan> GetBansFromGuild(ulong guildId, bool log = true) =>
            DatabaseEngine.GetBansFromGuild(guildId, log);
        public static void RemoveBanFromGuild(ulong banId, ulong guildId) =>
            DatabaseEngine.RemoveBanFromGuild(banId, guildId);

        public static void AddToAuditLog(ParsedCommand command, ulong guildId) =>
            DatabaseEngine.AddToAuditLog(command, guildId);
        public static List<AuditCommand> GetAuditLog(ulong guildId) =>
            DatabaseEngine.GetAuditLog(guildId);

        public static void AddStatus(Status status) =>
            DatabaseEngine.AddStatus(status);

        public static Giveaway CreateGiveaway(Giveaway giveaway, ulong guildId) =>
            DatabaseEngine.CreateGiveaway(giveaway, guildId);

        public static Giveaway UpdateOrCreateGiveaway(Giveaway giveaway, ulong guildId) =>
            DatabaseEngine.UpdateOrCreateGiveaway(giveaway, guildId);

        public static List<Giveaway> GetGiveaways(ulong guildId) =>
            DatabaseEngine.GetGiveaways(guildId);

        public static void DeleteGiveaway(Giveaway giveaway, ulong guildId) =>
            DatabaseEngine.DeleteGiveaway(giveaway, guildId);
        
        // TODO: Figure out what to do with this
        /// <summary>
        /// Create a VerificationEvent and add it to the database
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="guildId"></param>
        public static void AddVerificationEvent(ulong userId, ulong guildId) =>
            DatabaseEngine.AddVerification(userId, guildId);

        /// <summary>
        /// Get all quotes from the database for a guild
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public static List<Quote> GetAllQuotes(ulong guildId) =>
            DatabaseEngine.GetAllQuotes(guildId);

        /// <summary>
        /// Add or update an ExceptionReport to the database, and open an issue on GitHub if possible
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        public static ExceptionReport AddOrUpdateExceptionReport(ExceptionReport report)
        {
            report = DatabaseEngine.AddOrUpdateExceptionReport(report);

            if (!report.Reported && report.Count >= 5 && !string.IsNullOrEmpty(GlobalConfig.GithubToken))
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
                        $"Reporting Build (if available): {Program.BuildId}\n";

                    var issue = client.Issue.Create(client.User.Current().Result.Login, "GenericBot", issueToCreate).Result;
                    report.Reported = true;
                    report = DatabaseEngine.AddOrUpdateExceptionReport(report);
                }
                catch (Exception ex)
                {
                    Logger.LogGenericMessage("An error occured reporting to github. Please check your credentials and that there is a repo \"GenericBot\" associated with your account.");
                    Logger.LogGenericMessage(ex.Message + "\n" + ex.StackTrace);
                }
            }

            return report;
        }
    }
}
