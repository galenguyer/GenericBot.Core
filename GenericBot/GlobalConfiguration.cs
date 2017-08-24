using System.Collections.Generic;

namespace GenericBot
{
    public class GlobalConfiguration
    {
        //Bot Token
        public string Token { get; set; }
        //Default Prefix to be used
        public string DefaultPrefix { get; set; }
        //Default value for executing edited commands
        public bool DefaultExecuteEdits { get; set; }
        //Bot Owner's ID (Let's not use Discord for this, it's slow)
        public ulong OwnerId { get; set; }
        //Global Admins (Have as much power as owner)
        public List<ulong> GlobalAdminIds { get; set; } 
        
        //Blacklist Options
        //Specific Guilds (for spamming/abuse)
        public List<ulong> BlacklistedGuildIds { get; set; }
        //Owners (can't have it on any guild they own)
        public List<ulong> BlacklistedOwnerIds { get; set; }
        //Total Blacklist of Death (no ownership OR commands)
        public List<ulong> BlacklistedUserIds { get; set; }
    }
}