using System;
using System.Collections.Generic;
using Discord.WebSocket;

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

        public GlobalConfiguration()
        {
            Token = "";
            DefaultPrefix = ">";
            DefaultExecuteEdits = true;
            OwnerId = new ulong();
            GlobalAdminIds = new List<ulong>();
            
            BlacklistedGuildIds = new List<ulong>();
            BlacklistedOwnerIds = new List<ulong>();
            BlacklistedUserIds = new List<ulong>();
        }
        
        //Add To various blacklists
        //return values
        //0 SUCCESS, 1 FORBIDDEN, 2 ALREADY CONTAINED
        //Add a guildId to the guild blacklist
        public int AddGuildBlacklist(DiscordSocketClient socketClient, ulong guildId)
        {
            try
            {
                var guild = socketClient.GetGuild(guildId);
                if (OwnerId.Equals(guild.OwnerId))
                {
                    Console.WriteLine("Cannot Blacklist a Guild owned by the Bot Owner");
                    return 1;
                }
                if (GlobalAdminIds.Contains(guild.OwnerId))
                {
                    Console.WriteLine("Cannot Blacklist a Guild owned by a Global Admin");
                    return 1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (BlacklistedGuildIds.Contains(guildId))
            {
                Console.WriteLine("Requested Guild is already Blacklisted");
                return 2;
            }
            
            BlacklistedGuildIds.Add(guildId);
            Console.WriteLine($"Added {guildId} to the Guild Blacklist");
            return 0;
        } 
        //Add a userId to the guildOwner blacklist
        public int AddOwnerBlacklist(ulong userId)
        {
            if (OwnerId.Equals(userId))
            {
                Console.WriteLine("Cannot Blacklist Bot Owner");
                return 1;
            }
            if (GlobalAdminIds.Contains(userId))
            {
                Console.WriteLine("Cannot Blacklist Global Admin");
                return 1;
            }
            if (BlacklistedOwnerIds.Contains(userId))
            {
                Console.WriteLine("Requested GuildOwner is already Blacklisted");
                return 2;
            }
            
            BlacklistedOwnerIds.Add(userId);
            Console.WriteLine($"Added {userId} to the Guild Owner Blacklist");
            return 0;
        }
        //Add a userId to the User blacklist
        public int AddUserBlacklist(ulong userId)
        {
            if (OwnerId.Equals(userId))
            {
                Console.WriteLine("Cannot Blacklist Bot Owner");
                return 1;
            }
            if (GlobalAdminIds.Contains(userId))
            {
                Console.WriteLine("Cannot Blacklist Global Admin");
                return 1;
            }
            if (BlacklistedOwnerIds.Contains(userId))
            {
                Console.WriteLine("Requested User is already Blacklisted");
                return 2;
            }
            
            BlacklistedOwnerIds.Add(userId);
            Console.WriteLine($"Added {userId} to the User Blacklist");
            return 0;
        }
    }
}