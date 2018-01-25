using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using GenericBot.Entities;
using LiteDB;

namespace GenericBot
{
    public static class UserEventHandler
    {
        public static async Task UserUpdated(SocketGuildUser beforeUser, SocketGuildUser afterUser)
        {
            if (beforeUser.Username != afterUser.Username)
            {
                using (var db = new LiteDatabase(GenericBot.DBConnectionString))
                {
                    var col = db.GetCollection<DBGuild>("userDatabase");
                    col.EnsureIndex(c => c.ID, true);
                    DBGuild guildDb;
                    if(col.Exists(g => g.ID.Equals(afterUser.Guild.Id)))
                        guildDb = col.FindOne(g => g.ID.Equals(afterUser.Guild.Id));
                    else guildDb = new DBGuild (afterUser.Guild);
                    if (guildDb.Users.Any(u => u.ID.Equals(afterUser.Id))) // if already exists
                    {
                        guildDb.Users.Find(u => u.ID.Equals(afterUser.Id)).AddUsername(afterUser.Username);
                    }
                    else
                    {
                        guildDb.Users.Add(new DBUser(afterUser));
                    }
                    col.Upsert(guildDb);
                    db.Dispose();
                }
            }
            if (beforeUser.Nickname != afterUser.Nickname)
            {
                using (var db = new LiteDatabase(GenericBot.DBConnectionString))
                {
                    var col = db.GetCollection<DBGuild>("userDatabase");
                    col.EnsureIndex(c => c.ID, true);
                    DBGuild guildDb;
                    if(col.Exists(g => g.ID.Equals(afterUser.Guild.Id)))
                        guildDb = col.FindOne(g => g.ID.Equals(afterUser.Guild.Id));
                    else guildDb = new DBGuild (afterUser.Guild);
                    if (guildDb.Users.Any(u => u.ID.Equals(afterUser.Id))) // if already exists
                    {
                        guildDb.Users.Find(u => u.ID.Equals(afterUser.Id)).AddNickname(afterUser);
                    }
                    else
                    {
                        guildDb.Users.Add(new DBUser(afterUser));
                    }
                    col.Upsert(guildDb);
                    db.Dispose();
                }
            }
        }

        public static async Task UserJoined(SocketGuildUser user)
        {
            List<string> Warnings = new List<string>();

            #region Database

            using (var db = new LiteDatabase(GenericBot.DBConnectionString))
            {
                var col = db.GetCollection<DBGuild>("userDatabase");
                col.EnsureIndex(c => c.ID, true);
                DBGuild guildDb;
                if(col.Exists(g => g.ID.Equals(user.Guild.Id)))
                    guildDb = col.FindOne(g => g.ID.Equals(user.Guild.Id));
                else guildDb = new DBGuild (user.Guild);
                if (guildDb.Users.Any(u => u.ID.Equals(user.Id))) // if already exists
                {
                    guildDb.Users.Find(u => u.ID.Equals(user.Id)).AddUsername(user.Username);
                }
                else
                {
                    guildDb.Users.Add(new DBUser(user));
                }
                col.Upsert(guildDb);
                Warnings.AddRange(guildDb.Users.Find(u => u.ID.Equals(user.Id)).Warnings);
                db.Dispose();
            }

            #endregion Databasae
        }
    }
}
