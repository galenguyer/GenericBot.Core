using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Timers;
using Discord.WebSocket;
using LiteDB;

namespace GenericBot.Entities
{
    public class DBGuild
    {
        public ulong ID { get; set; }
        public List<DBUser> Users { get; set; }

        public DBGuild()
        {

        }

        public DBGuild(SocketGuild guild)
        {
            this.ID = guild.Id;
            this.Users = new List<DBUser>();
        }

        public async void Save()
        {
            using (var db = new LiteDatabase(GenericBot.DBConnectionString))
            {
                var col = db.GetCollection<DBGuild>("userDatabase");
                col.EnsureIndex(c => c.ID, true);
                col.Upsert(this);
                db.Dispose();
            }
        }
        public DBGuild GetDBGuildFromId(ulong guildId)
        {
            if (GenericBot.LoadedGuilds.Values.Any(v => v.ID.Equals(guildId)))
            {
                GenericBot.GuildDBTimers[guildId] = DateTimeOffset.Now.AddMinutes(1);
                return GenericBot.LoadedGuilds[guildId];
            }
            else
            {
                using (var db = new LiteDatabase(GenericBot.DBConnectionString))
                {
                    DBGuild tempdb;
                    var col = db.GetCollection<DBGuild>("userDatabase");
                    col.EnsureIndex(c => c.ID, true);
                    if (col.Exists(c => c.ID.Equals(guildId)))
                    {
                        tempdb = col.FindOne(c => c.ID.Equals(guildId));
                    }
                    else
                    {
                        tempdb = new DBGuild(){ID = guildId, Users = new List<DBUser>()};
                    }
                    db.Dispose();
                    GenericBot.GuildDBTimers.TryAdd(guildId, DateTimeOffset.Now.AddMinutes(1));
                    return tempdb;
                }
            }
        }

        public DBUser GetUser(ulong id)
        {
            DBUser res;
            if (Users.HasElement(u => u.ID.Equals(id), out res))
            {
                return res;
            }
            else
            {
                Users.Add(new DBUser(){ID = id});
                return Users.First(u => u.ID == id);
            };
        }
    }
}
