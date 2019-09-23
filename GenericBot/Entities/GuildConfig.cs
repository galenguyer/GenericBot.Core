using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
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
        [BsonId]
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public List<ulong> AdminRoleIds { get; set; }
        public List<ulong> ModRoleIds { get; set; }
        public List<ulong> UserRoleIds { get; set; }
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<string, List<ulong>> UserRoles { get; set; }
        public List<ulong> AutoRoleIds { get; set; }
        public ulong UserLogChannelId { get; set; }
        public List<ulong> MessageLoggingIgnoreChannels { get; set; }
        public ulong VerifiedRole { get; set; }
        public string VerifiedMessage { get; set; }
        public AntiSpamLevel AntispamLevel { get; set; }

        public GuildConfig(ulong id)
        {
            Id = id;
            VerifiedRole = 0;
            AntispamLevel = AntiSpamLevel.None;
            MessageLoggingIgnoreChannels = new List<ulong>();
            AdminRoleIds = new List<ulong>();
            ModRoleIds = new List<ulong>();
            UserRoleIds = new List<ulong>();
            UserRoles = new Dictionary<string, List<ulong>>();
            AutoRoleIds = new List<ulong>();
        }

        public GuildConfig Save()
        {
            return this;
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
