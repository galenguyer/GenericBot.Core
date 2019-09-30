using Discord;
using Discord.WebSocket;
using GenericBot.CommandModules;
using GenericBot.Database;
using GenericBot.Entities;
using System.Collections.Generic;
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
        public static Logger Logger { get; private set; }
        public static MongoEngine MongoEngine { get; private set; }

        private static List<GuildConfig> LoadedGuildConfigs;

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
            InitializeCache();

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
            //DiscordClient.GuildAvailable += GuildEventHandler.GuildLoaded;
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
            Commands.AddRange(new QuickCommands().GetQuickCommands());

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
            if (LoadedGuildConfigs.Any(c => c.Id == GuildId))
            {
                return LoadedGuildConfigs.Find(c => c.Id == GuildId);
            }
            else
            {
                LoadedGuildConfigs.Add(MongoEngine.GetGuildConfig(GuildId));
                return GetGuildConfig(GuildId); // Now that it's cached
            }
        }
        public static async Task<GuildConfig> SaveGuildConfig(GuildConfig guildConfig)
        {
            if (LoadedGuildConfigs.Any(c => c.Id == guildConfig.Id))
                LoadedGuildConfigs.RemoveAll(c => c.Id == guildConfig.Id);
            LoadedGuildConfigs.Add(guildConfig);

            return MongoEngine.SaveGuildConfig(guildConfig);
        }
        public static async Task<List<CustomCommand>> GetCustomCommands(ulong guildId)
        {
            if (CustomCommands.ContainsKey(guildId))
                return CustomCommands[guildId];
            else
                return MongoEngine.GetCustomCommands(guildId);
        }
        public static async Task<CustomCommand> SaveCustomCommand(CustomCommand command, ulong guildId)
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
        public static Quote AddQuote(string quote, ulong guildId)
        {
            return MongoEngine.AddQuote(quote, guildId);
        }
        public static bool RemoveQuote(int id, ulong guildId)
        {
            return MongoEngine.RemoveQuote(id, guildId);
        }
        public static Quote GetQuote(string quote, ulong guildId)
        {
            var quotes = MongoEngine.GetAllQuotes(guildId);

            if (string.IsNullOrEmpty(quote))
            {
                return quotes.GetRandomItem();
            }
            else
            {
                return quotes.Where(q => q.Content.ToLower().Contains(quote.ToLower())).ToList().GetRandomItem();
            }
        }

        private static void InitializeCache()
        {
            foreach (var stringId in MongoEngine.GetGuildIdsFromDb())
            {
                if (ulong.TryParse(stringId, out ulong guildId))
                {
                    LoadedGuildConfigs.Add(GetGuildConfig(guildId));
                    Logger.LogGenericMessage($"Loaded GuildConfig for {stringId}");
                    CustomCommands.Add(guildId, GetCustomCommands(guildId).Result);
                    Logger.LogGenericMessage($"Loaded Custom Commands for {stringId}");
                }
            }
        }
    }
}
