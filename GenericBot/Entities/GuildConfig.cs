using System;
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
        public ulong UserLogChannelId;
        public bool UserLogTimestamp;
        public string UserJoinedMessage;
        public string UserLeftMessage;
        public bool UserJoinedShowModNotes;
        public List<ulong> MessageLoggingIgnoreChannels = new List<ulong>();
        public ulong VerifiedRole = 0;
        public string VerifiedMessage;
        public ulong FourChannelId;
        public string Prefix;
        public bool AllowTwitter = false;
        public Giveaway Giveaway;

        public List<ChannelMute> ChannelMutes = new List<ChannelMute>();
        public List<CustomCommand> CustomCommands;
        public List<CustomAlias> CustomAliases;

        public Dictionary<ulong, Poll> Polls = new Dictionary<ulong, Poll>();
        public Dictionary<ulong, ulong> VoiceChannelRoles = new Dictionary<ulong, ulong>();
        public List<ulong> ProbablyMutedUsers = new List<ulong>();
        public ulong MutedRoleId = 0;

        public GuildConfig(ulong id)
        {
            GuildId = id;
            AdminRoleIds = new List<ulong>();
            ModRoleIds = new List<ulong>();
            UserRoleIds = new List<ulong>();
            CustomCommands = new List<CustomCommand>();
            CustomAliases = new List<CustomAlias>();

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

        public bool AddBan(GenericBan ban)
        {
            try
            {
                var guild = GenericBot.DiscordClient.GetGuild(GuildId);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

    public class GenericBan
    {
        public ulong Id;
        public DateTimeOffset BannedUntil;
        public bool IsPermanent;
        public string Reason;

        public GenericBan(ulong id, string reason, string until = null, bool permanent = false)
        {
            this.Id = id;
            this.BannedUntil = !permanent ? DateTimeOffset.Parse(until) : DateTimeOffset.MaxValue;
            this.IsPermanent = permanent;
            this.Reason = reason;
        }
    }
}
