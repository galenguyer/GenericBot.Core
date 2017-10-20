using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
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
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            // Don't handle the command if it is a system message
            var message = parameterMessage as SocketUserMessage;
            if (message == null) return;

            var commandInfo = ParseMessage(parameterMessage);

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

            if (!String.IsNullOrEmpty(GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix))
                pref = GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix;

            if (!message.StartsWith(pref)) return null;

            message = message.Substring(pref.Length);

            string commandId = message.Split(' ')[0].ToLower();

            var cmd = GenericBot.Commands.Find(c => commandId.Equals(c.Name) || c.Aliases.Any(a => commandId.Equals(a)));

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
