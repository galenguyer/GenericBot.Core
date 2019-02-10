using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GenericBot.CommandModules;
using GenericBot.Entities;

namespace GenericBot
{
    public class CommandHandler
    {
        private DiscordShardedClient _client = GenericBot.DiscordClient;
        private IServiceProvider _map;

        public Task Install(IServiceProvider map)
        {
            _map = map;
            // Create Command Service, inject it into Dependency Map
            _client = map.GetService(typeof(DiscordShardedClient)) as DiscordShardedClient;

            GenericBot.Commands.AddRange(new BotCommands().GetBotCommands());
            GenericBot.Commands.AddRange(new HelpModule().GetHelpCommands());
            GenericBot.Commands.AddRange(new TestCommands().GetTestCommands());
            GenericBot.Commands.AddRange(new ConfigCommands().GetConfigComamnds());
            GenericBot.Commands.AddRange(new RoleCommands().GetRoleCommands());
            GenericBot.Commands.AddRange(new ModCommands().GetModCommands());
            GenericBot.Commands.AddRange(new FunCommands().GetFunCommands());
            GenericBot.Commands.AddRange(new SocialCommands().GetSocialCommands());
            GenericBot.Commands.AddRange(new MuteCommands().GetMuteCommands());
            GenericBot.Commands.AddRange(new BanCommands().GetBanCommands());
            GenericBot.Commands.AddRange(new CustomCommandCommands().GetCustomCommands());
            GenericBot.Commands.AddRange(new CardCommands().GetCardCommands());
            GenericBot.Commands.AddRange(new QuickCommands().GetQuickCommands());
            GenericBot.Commands.AddRange(new PointsCommands().GetPointsCommands());
            GenericBot.Commands.AddRange(new InfoCommands().GetInfoCommands());
            GenericBot.Commands.AddRange(new AnalyticsCommandLoader().GetAnalyticsCommand());
            GenericBot.Commands.AddRange(new NoPolymer().GetPolyCommands());

            Console.WriteLine(GenericBot.Commands.Select(c => c.Name).Aggregate((i, j) => i + ", " + j));

            return Task.CompletedTask;
        }

        public static ParsedCommand ParseMessage(SocketMessage msg)
        {
            ParsedCommand parsedCommand = new ParsedCommand();

            parsedCommand.Message = msg;

            string message = msg.Content;

            string pref = GenericBot.GlobalConfiguration.DefaultPrefix;

            if (msg.Channel is IDMChannel) goto DMC;

            if (!String.IsNullOrEmpty(GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix))
                pref = GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix;

            DMC:

            if (!message.StartsWith(pref)) return null;

            message = message.Substring(pref.Length);

            string commandId = message.Split(' ')[0].ToLower();

            Command cmd = new Command("tempCommand");

            if (GenericBot.Commands.HasElement(c => commandId.Equals(c.Name) || c.Aliases.Any(a => commandId.Equals(a)) ||
                                                    GenericBot.GuildConfigs[msg.GetGuild().Id].CustomAliases.Any(a => a.Alias == commandId) &&
                                                    c.Name == GenericBot.GuildConfigs[msg.GetGuild().Id].CustomAliases.First(a => a.Alias == commandId).Command, out cmd))
            {
                parsedCommand.Command = cmd;
            }
            else
            {
                parsedCommand.Command = null;
            }

            parsedCommand.Name = commandId;

            try
            {
                string param = message.Substring(commandId.Length);
                parsedCommand.Parameters = param.Split().Where(p => !string.IsNullOrEmpty(p.Trim())).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return parsedCommand;
        }

        public static string GetParameterString(SocketMessage msg)
        {
            ParsedCommand parsedCommand = new ParsedCommand();

            parsedCommand.Message = msg;

            string message = msg.Content;

            string pref = GenericBot.GlobalConfiguration.DefaultPrefix;

            if (msg.Channel is IDMChannel) goto DMC;

            if (!String.IsNullOrEmpty(GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix))
                pref = GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix;

            DMC:

            if (!message.StartsWith(pref)) return null;

            message = message.Substring(pref.Length);

            string commandId = message.Split(' ')[0].ToLower();

            Command cmd = new Command("tempCommand");

            if (GenericBot.Commands.HasElement(c => commandId.Equals(c.Name) || c.Aliases.Any(a => commandId.Equals(a)) ||
                                                    GenericBot.GuildConfigs[msg.GetGuild().Id].CustomAliases.Any(a => a.Alias == commandId) &&
                                                    c.Name == GenericBot.GuildConfigs[msg.GetGuild().Id].CustomAliases.First(a => a.Alias == commandId).Command, out cmd))
            {
                parsedCommand.Command = cmd;
            }
            else
            {
                parsedCommand.Command = null;
            }

            parsedCommand.Name = commandId;
            return message.Substring(commandId.Length);
        }

    }
}
