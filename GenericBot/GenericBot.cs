using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

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
        public static Encryption Crypto = new Encryption();

        static void Main(string[] args)
        {
            Logger = new Logger(GetStringSha256Hash(DateTime.UtcNow.ToString()));
            GlobalConfiguration = new GlobalConfiguration().Load();
            GuildConfigs =
                JsonConvert.DeserializeObject<Dictionary<ulong, GuildConfig>>(File.ReadAllText("files/guildConfigs.json"));
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
            }

            var serviceProvider = ConfigureServices();

            var _handler = new CommandHandler();
            await _handler.Install(serviceProvider);

            // Block this program until it is closed.
            await Task.Delay(-1);
        }

        private async Task OnReady()
        {
            foreach (var guild in DiscordClient.Guilds.Where(g => !GuildConfigs.Keys.Contains(g.Id)))
            {
                GuildConfigs.Add(guild.Id, new GuildConfig(guild.Id));
            }
            File.WriteAllText("files/guildConfigs.json", JsonConvert.SerializeObject(GuildConfigs, Formatting.Indented));
            await Logger.LogGenericMessage($"Loaded {GuildConfigs.Count} Configs on Startup");
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(DiscordClient);
            var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
            return provider;
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
    }
}
