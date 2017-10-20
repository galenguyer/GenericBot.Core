using System.Collections.Generic;

namespace GenericBot.Entities
{
    public class GuildConfig
    {
        public ulong GuildId;
        public List<ulong> AdminRoleIds;
        public List<ulong> ModRoleIds;
        public List<ulong> GiveableRoleIds;
        public Dictionary<Command, List<string>> LocalAliases;
        public string Prefix;

        public GuildConfig(ulong id)
        {
            GuildId = id;
            AdminRoleIds = new List<ulong>();
            ModRoleIds = new List<ulong>();
            GiveableRoleIds = new List<ulong>();
            Prefix = GenericBot.GlobalConfiguration.DefaultPrefix;
        }
    }
}
