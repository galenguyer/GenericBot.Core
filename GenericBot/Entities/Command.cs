using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace GenericBot.Entities
{
    public class Command
    {
        public enum PermissionLevels
        {
            User,
            Moderator,
            Admin,
            GuildOwner,
            GlobalAdmin,
            BotOwner
        }

        public string Name;
        public List<string> Aliases = new List<string>();
        public string Description = "Not Available";
        public string Usage;
        public bool Delete = false;
        public bool SendTyping = true;
        public PermissionLevels RequiredPermission = PermissionLevels.User;

        public Command(string n)
        {
            this.Name = n.ToLower();
            this.Usage = this.Name;
        }

        public delegate Task ExecuteDelegate(ParsedCommand command);

        public ExecuteDelegate ToExecute = null;

        public async Task ExecuteCommand(ParsedCommand command)
        {
            if (GetPermissions((command.Message as SocketMessage).Author, (command.Message as SocketMessage).GetGuild().Id) < RequiredPermission)
                return;

            if (SendTyping) await command.Message.Channel.TriggerTypingAsync();
            if (Delete)
            {
                try
                {
                    await command.Message.DeleteAsync();
                }
                catch (Discord.Net.HttpException httpException)
                {
                    await GenericBot.Logger.LogErrorMessage(
                        $"Could Not Delete Message {command.Message.Id} CHANNELID {command.Message.Channel.Id}");
                }
            }

            try
            {
                await ToExecute(command);
            }
            catch (Exception ex)
            {
                if (command.Message.Author.Id == GenericBot.GetOwnerId())
                {
                    await (command.Message as SocketMessage).ReplyAsync("```\n" + $"{ex.Message}\n{ex.StackTrace}".SafeSubstring(1600) +
                                                      "\n```");
                }
                await GenericBot.Logger.LogErrorMessage(ex.Message+"\n"+ex.StackTrace);
            }
        }

        public PermissionLevels GetPermissions(SocketUser user, ulong guildId)
        {
            if (user.Id.Equals(GenericBot.GetOwnerId())) return PermissionLevels.BotOwner;
            else if (GenericBot.CheckGlobalAdmin(user.Id))
                return PermissionLevels.GlobalAdmin;
            else if(IsGuildAdmin(user, guildId))
                return PermissionLevels.GuildOwner;
            //else if (((SocketGuildUser) user).Roles.Select(r => r.Id).Intersect(GenericBot.GuildConfigs[guildId].AdminRoleIds).Any())
            //    return PermissionLevels.Admin;
            //else if (((SocketGuildUser) user).Roles.Select(r => r.Id).Intersect(GenericBot.GuildConfigs[guildId].ModRoleIds).Any())
            //    return PermissionLevels.Moderator;
            else return PermissionLevels.User;
        }
        private bool IsGuildAdmin(SocketUser user, ulong guildId)
        {
            var guild = GenericBot.GetGuid(guildId);
            if (guild.Owner.Id == user.Id)
                return true;
            else if (guild.GetUser(user.Id).Roles.Any(r => r.Permissions.Administrator))
                return true;
            return false;
        }

        public ParsedCommand ParseMessage(SocketMessage msg)
        {
            ParsedCommand parsedCommand = new ParsedCommand();
            parsedCommand.Message = msg;
            string message = msg.Content;
            string pref = GenericBot.GetPrefix();

            if (!message.StartsWith(pref)) return null;
            message = message.Substring(pref.Length);
            string commandId = message.Split(' ')[0].ToLower();
                        parsedCommand.Name = commandId;

            if (GenericBot.Commands.HasElement(c => commandId.Equals(c.Name) || c.Aliases.Any(a => commandId.Equals(a)), out Command cmd))
            {
                parsedCommand.RawCommand = cmd;
            }
            else
            {
                parsedCommand.RawCommand = null;
            }

            try
            {
                string param = message.Substring(commandId.Length);
                parsedCommand.ParameterString = param.Trim();
                parsedCommand.Parameters = param.Split().Where(p => !string.IsNullOrEmpty(p.Trim())).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return parsedCommand;
        }
    }
}
