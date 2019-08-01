using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace GenericBot.Entities
{
    public class GuildConfig
    {
        [Flags]
        public enum AntiSpamLevel
        {
            None = 1,
            Basic = 2, 
            Advanced = 4, /* Username filtering */
            Aggressive = 8,
            ActiveRaid = 16
        }
        public ulong GuildId { get; set; }
        public List<ulong> AdminRoleIds { get; set; }
        public List<ulong> ModRoleIds { get; set; }
        public List<ulong> UserRoleIds { get; set; }
        public List<ulong> AutoRoleIds { get; set; }
        public ulong UserLogChannelId { get; set; }
        public List<ulong> MessageLoggingIgnoreChannels { get; set; }
        public ulong VerifiedRole { get; set; }
        public string VerifiedMessage { get; set; }
        public AntiSpamLevel AntispamLevel { get; set; }

        public string PointsName { get; set; }
        public string PointsVerb { get; set; }
        public bool PointsEnabled { get; set; }
        public Dictionary<decimal, ulong> Levels { get; set; }

        public bool GlobalBanOptOut { get; set; }

        public ulong FourChannelId { get; set; }
        public string Prefix { get; set; }
        public Giveaway Giveaway { get; set; }

        public List<ChannelMute> ChannelMutes { get; set; }
        public List<CustomCommand> CustomCommands { get; set; }
        public List<CustomAlias> CustomAliases { get; set; }

        public List<ulong> ProbablyMutedUsers { get; set; }
        public ulong MutedRoleId { get; set; }
        public List<GenericBan> Bans { get; set; }

        public Dictionary<ulong, Discord.OverwritePermissions> ChannelOverrideDefaults { get; set; }

        public GuildConfig(ulong id)
        {
            GuildId = id;
            VerifiedRole = 0;
            AntispamLevel = AntiSpamLevel.None;
            MessageLoggingIgnoreChannels = new List<ulong>();
            PointsName = "point";
            PointsVerb = "used";
            PointsEnabled = false;
            AdminRoleIds = new List<ulong>();
            ModRoleIds = new List<ulong>();
            UserRoleIds = new List<ulong>();
            AutoRoleIds = new List<ulong>();
            CustomCommands = new List<CustomCommand>();
            CustomAliases = new List<CustomAlias>();
            Bans = new List<GenericBan>();
            ChannelMutes = new List<ChannelMute>();
            ProbablyMutedUsers = new List<ulong>();
            Levels = new Dictionary<decimal, ulong>();
            ChannelOverrideDefaults = new Dictionary<ulong, Discord.OverwritePermissions>();
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
            var gDb = GenericBot.mongoClient.GetDatabase($"{this.GuildId}");
            var coll = gDb.GetCollection<GuildConfig>("config");
            coll.InsertOne(this);
            return this;
        }

        public bool AddBan(GenericBan ban)
        {
            try
            {
                var guild = GenericBot.DiscordClient.GetGuild(GuildId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class GenericBan
    {
        public ulong Id;
        public ulong GuildId;
        public DateTimeOffset BannedUntil;
        public string Reason;

        public GenericBan(ulong userid, ulong guildid, string reason, DateTimeOffset time)
        {
            this.Id = userid;
            this.GuildId = guildid;
            this.BannedUntil = time;
            this.Reason = reason;
        }
    }
}
