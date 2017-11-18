using System.Collections.Generic;

namespace GenericBot.Entities
{
    public class GuildConfig
    {
        public ulong GuildId;
        public List<ulong> AdminRoleIds;
        public List<ulong> ModRoleIds;
        public List<ulong> UserRoleIds;
        public Dictionary<Command, List<string>> LocalAliases;
        public string Prefix;
        public bool AllowTwitter = true;
        public Giveaway Giveaway;

        public List<ChannelMute> ChannelMutes = new List<ChannelMute>();

        public GuildConfig(ulong id)
        {
            GuildId = id;
            AdminRoleIds = new List<ulong>();
            ModRoleIds = new List<ulong>();
            UserRoleIds = new List<ulong>();
            Prefix = GenericBot.GlobalConfiguration.DefaultPrefix;
        }
    }
}
