using System;
using System.Collections.Generic;
using System.IO;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace GenericBot.Entities
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
        //Use debug mode?
        public bool DebugMode { get; set; }

        public GlobalConfiguration()
        {
            Token = "";
            DefaultPrefix = ">";
            DefaultExecuteEdits = true;
            OwnerId = new ulong();
            GlobalAdminIds = new List<ulong>();
            DebugMode = false;

            BlacklistedGuildIds = new List<ulong>();
            BlacklistedOwnerIds = new List<ulong>();
            BlacklistedUserIds = new List<ulong>();
        }

        public GlobalConfiguration(DiscordSocketClient socketClient, string tok, string pref, bool edit)
        {
            Token = tok;
            DefaultPrefix = pref;
            DefaultExecuteEdits = edit;
            OwnerId = socketClient.GetApplicationInfoAsync().Result.Owner.Id;
            GlobalAdminIds.Add(OwnerId);
            DebugMode = false;

            BlacklistedGuildIds = new List<ulong>();
            BlacklistedOwnerIds = new List<ulong>();
            BlacklistedUserIds = new List<ulong>();
        }

        public void Save()
        {
            Directory.CreateDirectory("files");

            File.WriteAllText("files/config.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public GlobalConfiguration Load()
        {
            return JsonConvert.DeserializeObject<GlobalConfiguration>(File.ReadAllText("files/config.json"));
        }

        //Add Blacklist
        //0 SUCCESS, 1 FORBIDDEN, 2 ALREADY CONTAINED
        public int AddGuildBlacklist(DiscordSocketClient socketClient, ulong guildId)
        {
            try
            {
                var guild = socketClient.GetGuild(guildId);
                if (OwnerId.Equals(guild.OwnerId) || GlobalAdminIds.Contains(guild.OwnerId))
                    return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (BlacklistedGuildIds.Contains(guildId))
                return 2;

            BlacklistedGuildIds.Add(guildId);
            return 0;
        }
        public int AddOwnerBlacklist(ulong userId)
        {
            if (OwnerId.Equals(userId) || (GlobalAdminIds.Contains(userId)))
                return 1;
            if (BlacklistedOwnerIds.Contains(userId))
                return 2;

            BlacklistedOwnerIds.Add(userId);
            return 0;
        }
        public int AddUserBlacklist(ulong userId)
        {
            if (OwnerId.Equals(userId) || GlobalAdminIds.Contains(userId))
                return 1;
            if (BlacklistedOwnerIds.Contains(userId))
                return 2;

            BlacklistedOwnerIds.Add(userId);
            return 0;
        }

        //Remove Blacklist
        //0 SUCCESS, 1 NOT IN LIST
        public int RemoveGuildBlacklist(ulong guildId)
        {
            if (!BlacklistedGuildIds.Contains(guildId))
                return 1;

            BlacklistedGuildIds.Remove(guildId);
            return 0;
        }
        public int RemoveOwnerBlacklist(ulong ownerId)
        {
            if (!BlacklistedOwnerIds.Contains(ownerId))
                return 1;

            BlacklistedOwnerIds.Remove(ownerId);
            return 0;
        }
        public int RemoveUserBlacklist(ulong userId)
        {
            if (!BlacklistedUserIds.Contains(userId))
                return 1;

            BlacklistedUserIds.Remove(userId);
            return 0;
        }
    }
}
