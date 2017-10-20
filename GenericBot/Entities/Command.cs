using System;
using System.Collections.Generic;
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
            GlobalAdmin,
            BotOwner,
            Laterallyimpossible
        }

        public string Name = "";
        public List<string> Aliases = new List<string>(){""};
        public string Description = "Not Available";
        public string Usage = "Not Available";
        public bool Delete = false;
        public bool SendTyping = true;
        public PermissionLevels RequiredPermission = PermissionLevels.User;


        public delegate Task ExecuteDelegate(DiscordShardedClient client, SocketMessage msg, List<string> parameters);

        public ExecuteDelegate ToExecute = null;

        public async Task ExecuteCommand(DiscordShardedClient client, SocketMessage msg, List<string> parameters = null)
        {
            try
            {
                if (GetPermissions(msg.Author) < RequiredPermission) return;
                if (Delete)
                {
                    try
                    {
                        await msg.DeleteAsync();
                    }
                    catch (Discord.Net.HttpException httpException)
                    {
                        await GenericBot.Logger.LogErrorMessage(
                            $"Could Not Delete Message {msg.Id} CHANNELID {msg.Channel.Id}");
                    }
                }
                await ToExecute(client, msg, parameters);
            }
            catch (Exception ex)
            {

            }
        }

        private PermissionLevels GetPermissions(SocketUser user)
        {
            if (user.Id.Equals(GenericBot.GlobalConfiguration.OwnerId)) return PermissionLevels.BotOwner;
            else if (GenericBot.GlobalConfiguration.GlobalAdminIds.Contains(user.Id))
                return PermissionLevels.GlobalAdmin;
            else return PermissionLevels.User;
        }
    }

    public class CommandGroup
    {
        public string GroupName;
        public List<Command> GroupCommands;

    }
}
