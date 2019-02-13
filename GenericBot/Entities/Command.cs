using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
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
            BotOwner,
            Laterallyimpossible
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


        public delegate Task ExecuteDelegate(DiscordShardedClient client, SocketMessage msg, List<string> parameters);

        public ExecuteDelegate ToExecute = null;

        public async Task ExecuteCommand(DiscordShardedClient client, SocketMessage msg, List<string> parameters = null)
        {
            if (GetPermissions(msg.Author, (msg.Channel as SocketGuildChannel).Guild.Id) < RequiredPermission)
                return;
         
            // if (SendTyping)  await msg.Channel.TriggerTypingAsync();
            if (Delete)
            {
                try
                {
                    await msg.DeleteAsync();
                }
#pragma warning disable CS0168 // Variable is declared but never used
                catch (Discord.Net.HttpException httpException)
#pragma warning restore CS0168 // Variable is declared but never used
                {
                    await GenericBot.Logger.LogErrorMessage(
                        $"Could Not Delete Message {msg.Id} CHANNELID {msg.Channel.Id}");
                }
            }

            try
            {
                await ToExecute(client, msg, parameters);
            }
            catch (Exception ex)
            {
                if (msg.Author.Id == GenericBot.GlobalConfiguration.OwnerId)
                {
                    await msg.ReplyAsync("```\n" + $"{ex.Message}\n{ex.StackTrace}".SafeSubstring(1600) +
                                                      "\n```");
                }
                await GenericBot.Logger.LogErrorMessage(ex.Message+"\n"+ex.StackTrace);
            }
        }

        public PermissionLevels GetPermissions(SocketUser user, ulong guildId)
        {
            if (user.Id.Equals(GenericBot.GlobalConfiguration.OwnerId)) return PermissionLevels.BotOwner;
            else if (GenericBot.GlobalConfiguration.GlobalAdminIds.Contains(user.Id))
                return PermissionLevels.GlobalAdmin;
            else if(GenericBot.DiscordClient.GetGuild(guildId).Owner.Id == user.Id)
                return PermissionLevels.GuildOwner;
            else if (((SocketGuildUser) user).Roles.Select(r => r.Id).Intersect(GenericBot.GuildConfigs[guildId].AdminRoleIds).Any())
                return PermissionLevels.Admin;
            else if (((SocketGuildUser) user).Roles.Select(r => r.Id).Intersect(GenericBot.GuildConfigs[guildId].ModRoleIds).Any())
                return PermissionLevels.Moderator;
            else return PermissionLevels.User;
        }
    }
}
