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

        public async Task Install(IServiceProvider map)
        {
            _map = map;
            // Create Command Service, inject it into Dependency Map
            _client = map.GetService(typeof(DiscordShardedClient)) as DiscordShardedClient;

            _client.MessageUpdated += HandleEditedCommand;


            _client.GuildMemberUpdated += UserEventHandler.UserUpdated;
            _client.UserJoined += UserEventHandler.UserJoined;

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

            GenericBot.Commands.AddRange(new HackingCommands().GetHackedCommands());
            Console.WriteLine(GenericBot.Commands.Select(c => c.Name).Aggregate((i, j) => i+ ", " + j));
        }

        private async Task HandleEditedCommand(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            if (arg1.Value.Content == arg2.Content) return;

            if (GenericBot.GlobalConfiguration.DefaultExecuteEdits)
            {
                await MessageEventHandler.MessageRecieved(arg2);
            }

            var guildConfig = GenericBot.GuildConfigs[arg2.GetGuild().Id];

            if (guildConfig.UserLogChannelId == 0 || guildConfig.MessageLoggingIgnoreChannels.Contains(arg2.Channel.Id)
                                                  ||!arg1.HasValue) return;

            EmbedBuilder log = new EmbedBuilder()
                .WithTitle("Message Edited")
                .WithAuthor(new EmbedAuthorBuilder().WithName($"{arg2.Author} ({arg2.Author.Id})")
                    .WithIconUrl(arg2.Author.GetAvatarUrl() + " "))
                .WithColor(243, 110, 33)
                .WithCurrentTimestamp();

            log.AddField(new EmbedFieldBuilder().WithName("Channel").WithValue("#" + arg2.Channel.Name).WithIsInline(true));
            log.AddField(new EmbedFieldBuilder().WithName("Sent At").WithValue(arg1.Value.Timestamp.ToString(@"yyyy-MM-dd HH:mm.ss") + "GMT").WithIsInline(true));

            log.WithDescription("**Before:** " + arg1.Value.Content.SafeSubstring(1000) + "\n\n**After:** " + arg2.Content.SafeSubstring(1000));
            foreach (var uid in arg1.Value.MentionedUserIds)
            {
                log.Description = log.Description.Replace($"<@!{uid}>", "@" + GenericBot.DiscordClient.GetUser(uid).Username);
            }
            foreach (var uid in arg2.MentionedUsers)
            {
                log.Description = log.Description.Replace($"<@!{uid.Id}>", "@" + uid.Username);
            }

            await (arg2 as SocketMessage).GetGuild().GetTextChannel(guildConfig.UserLogChannelId).SendMessageAsync("", embed: log.Build());
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

            if(GenericBot.Commands.HasElement(c => commandId.Equals(c.Name) || c.Aliases.Any(a => commandId.Equals(a)) ||
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

            //Console.WriteLine($"Command: {parsedCommand.Command.Name} Name: {parsedCommand.Name} Parameters: {parsedCommand.Parameters.Count}");

            return parsedCommand;
        }
    }
}
