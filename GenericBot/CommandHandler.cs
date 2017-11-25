using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GenericBot.CommandModules;
using GenericBot.Entities;
using Newtonsoft.Json;

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

            _client.MessageReceived += HandleCommand;
            //_client.MessageUpdated += HandleEditedCommand;

            _client.JoinedGuild += OnJoinedGuild;
            _client.LeftGuild += OnLeftGuild;


            GenericBot.Commands.AddRange(new BotCommands().GetBotCommands());
            GenericBot.Commands.AddRange(new HelpModule().GetHelpCommands());
            GenericBot.Commands.AddRange(new TestCommands().GetTestCommands());
            GenericBot.Commands.AddRange(new ConfigCommands().GetConfigComamnds());
            GenericBot.Commands.AddRange(new RoleCommands().GetRoleCommands());
            GenericBot.Commands.AddRange(new ModCommands().GetModCommands());
            GenericBot.Commands.AddRange(new FunCommands().GetFunCommands());
            GenericBot.Commands.AddRange(new SocialCommands().GetSocialCommands());
            GenericBot.Commands.AddRange(new MuteCommands().GetMuteCommands());
            GenericBot.Commands.AddRange(new CustomCommandCommands().GetCustomCommands());
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            // Don't handle the command if it is a system message
            var message = parameterMessage as SocketUserMessage;

//            if (message?.Author.Id != 169918990313848832 && message?.Author.Id != 354739264359104514) return;

            var commandInfo = ParseMessage(parameterMessage);

            CustomCommand custom = new CustomCommand();

            if (GenericBot.GuildConfigs[parameterMessage.GetGuild().Id].CustomCommands
                    .HasElement(c => c.Name == commandInfo.Name, out custom) ||
                GenericBot.GuildConfigs[parameterMessage.GetGuild().Id].CustomCommands
                    .HasElement(c => c.Aliases.Contains(commandInfo.Name), out custom))
            {
                if (custom.Delete)
                {
                    await parameterMessage.DeleteAsync();
                }
                await parameterMessage.ReplyAsync(custom.Response);
            }

            commandInfo.Command.ExecuteCommand(_client, message, commandInfo.Parameters).FireAndForget();
        }

        public async Task OnJoinedGuild(SocketGuild guild)
        {
            if (GenericBot.GuildConfigs.ContainsKey(guild.Id))
            {
                await GenericBot.Logger.LogGenericMessage($"Re-Joined Guild {guild.Id}({guild.Name}) Owned By {guild.Owner.Id}({guild.Owner})");
                string pref = !string.IsNullOrEmpty(GenericBot.GuildConfigs[guild.Id].Prefix)
                    ? GenericBot.GuildConfigs[guild.Id].Prefix
                    : GenericBot.GlobalConfiguration.DefaultPrefix;
                await guild.Owner.GetOrCreateDMChannelAsync().Result.SendMessageAsync($"I'm back on `{guild.Name}`! " +
                                                                                      $"If you don't remember me, go ahead and do `{pref}config` or `{pref}help`.");
            }
            else
            {
                await GenericBot.Logger.LogGenericMessage($"Joined Guild {guild.Id}({guild.Name}) Owned By {guild.Owner.Id}({guild.Owner})");
                await GenericBot.Logger.LogGenericMessage($"Creating GuildConfig for {guild.Id}");
                string joinMsg =
                    $"Hey, awesome! Looks like someone (maybe even you) just invited me to your server, {guild.Name}! " +
                    $"If you wanna see everything I can do out of the box, do `{GenericBot.GlobalConfiguration.DefaultPrefix}help. " +
                    $"To set me up, do `{GenericBot.GlobalConfiguration.DefaultPrefix}config`";
                await guild.Owner.GetOrCreateDMChannelAsync().Result.SendMessageAsync(joinMsg);
                GenericBot.GuildConfigs.Add(guild.Id, new GuildConfig(guild.Id));
                File.WriteAllText("files/guildConfigs.json", JsonConvert.SerializeObject(GenericBot.GuildConfigs, Formatting.Indented));
            }
        }

        public async Task OnLeftGuild(SocketGuild guild)
        {
            await GenericBot.Logger.LogGenericMessage($"Left {guild.Id}({guild.Name})");
        }

        public ParsedCommand ParseMessage(SocketMessage msg)
        {
            ParsedCommand parsedCommand = new ParsedCommand();

            string message = msg.Content;

            string pref = GenericBot.GlobalConfiguration.DefaultPrefix;

            if (msg.Channel is IDMChannel) goto DMC;

            if (!String.IsNullOrEmpty(GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix))
                pref = GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix;

            DMC:

            if (!message.StartsWith(pref)) return null;

            message = message.Substring(pref.Length);

            string commandId = message.Split(' ')[0].ToLower();

            var cmd = GenericBot.Commands.First(c => commandId.Equals(c.Name) || c.Aliases.Any(a => commandId.Equals(a)) ||
                                                     (GenericBot.GuildConfigs[msg.GetGuild().Id].CustomAliases.Any(a => a.Alias == commandId) &&
                                                      c.Name == GenericBot.GuildConfigs[msg.GetGuild().Id].CustomAliases.First(a => a.Alias == commandId).Command));

            parsedCommand.Command = cmd;

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
