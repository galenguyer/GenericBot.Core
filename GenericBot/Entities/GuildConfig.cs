using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace GenericBot.Entities
{
    public class GuildConfig
    {
        public ulong GuildId;
        public List<ulong> AdminRoleIds;
        public List<ulong> ModRoleIds;
        public List<ulong> UserRoleIds;
        public bool LogUserEvents;
        public ulong UserLogChannelId;
        public bool UserLogTimestamp;
        public string UserJoinedMessage;
        public string UserLeftMessage;
        public bool UserJoinedShowModNotes;
        public string Prefix;
        public bool AllowTwitter = false;
        public Giveaway Giveaway;

        public List<ChannelMute> ChannelMutes = new List<ChannelMute>();
        public List<CustomCommand> CustomCommands;
        public List<CustomAlias> CustomAliases;

        public Dictionary<ulong, Poll> Polls = new Dictionary<ulong, Poll>();

        public GuildConfig(ulong id)
        {
            GuildId = id;
            AdminRoleIds = new List<ulong>();
            ModRoleIds = new List<ulong>();
            UserRoleIds = new List<ulong>();
            CustomCommands = new List<CustomCommand>();
            CustomAliases = new List<CustomAlias>();

            LogUserEvents = false;
            UserLogTimestamp = true;
            UserJoinedMessage = "{mention} (`{id}` | `{username}`) **joined** the server.";
            UserLeftMessage = "**{username}** (`{id}`) **left** the server.";
            UserJoinedShowModNotes = false;

            Prefix = "";
        }

        public GuildConfig Save()
        {
            if (!GenericBot.GuildConfigs.Any(kvp => kvp.Key.Equals(GuildId)))
            {
                GenericBot.GuildConfigs.Add(GuildId, this);
            }
            else
            {
                GenericBot.GuildConfigs[GuildId] = this;
            }
            File.WriteAllText($"files/guildConfigs/{this.GuildId}.json", JsonConvert.SerializeObject(this, Formatting.Indented));
            return this;
        }
    }
}
