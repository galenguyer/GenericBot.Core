using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using LiteDB;
using Newtonsoft.Json;

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
                db.Dispose();
            }

            #endregion Databasae

            #region Logging

            var guildConfig = GenericBot.GuildConfigs[user.Guild.Id];


            if (guildConfig.UserLogChannelId == 0) return;

            try
            {
                string message = guildConfig.UserJoinedMessage.Replace("{id}", user.Id.ToString()).Replace("{username}", user.ToString()).Replace("{mention}", user.Mention);
                if (guildConfig.UserLogTimestamp == true)
                {
                    message = $"`{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")}` {message}";
                }
                if (guildConfig.UserJoinedShowModNotes == true && (DateTimeOffset.Now - user.CreatedAt).TotalDays < 7)
                {
                    message = $"{message} **New User:** Account made `{Math.Floor((DateTimeOffset.Now - user.CreatedAt).TotalDays)}` days `{Math.Floor((double) (DateTimeOffset.Now - user.CreatedAt).Hours)}` hours ago.";
                }
                var logMessage = user.Guild.GetTextChannel(guildConfig.UserLogChannelId).SendMessageAsync(message).Result;

                DBGuild guildDb = new DBGuild().GetDBGuildFromId((user).Guild.Id);
                DBUser usr = guildDb.Users.First(u => u.ID.Equals(user.Id));

                if (guildConfig.UserJoinedShowModNotes && !usr.Warnings.Empty())
                {
                    await logMessage.ModifyAsync(m =>
                        m.Content = $"{message}\n**{usr.Warnings.Count}** Warnings: {usr.Warnings.reJoin(", ")}");
                }            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            #endregion Logging
        }

        public static async Task UserChangedVc(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            var config = GenericBot.GuildConfigs[(user as SocketGuildUser).Guild.Id];
            KeyValuePair<ulong, ulong> chanrole;
            if (before.VoiceChannel != null)
            {
                if (GenericBot.GuildConfigs[(user as SocketGuildUser).Guild.Id].VoiceChannelRoles
                    .HasElement(kvp => kvp.Key.Equals(before.VoiceChannel.Id), out chanrole))
                {
                    try
                    {
                        (user as IGuildUser).RemoveRoleAsync((user as SocketGuildUser).Guild.Roles.
                            FirstOrDefault(r => r.Id == chanrole.Value));
                    }
                    catch (Exception e)
                    {
                        GenericBot.Logger.LogErrorMessage(e.Message +"\n"  +e.StackTrace);
                    }
                }
            }
            if (after.VoiceChannel != null)
            {
              if (GenericBot.GuildConfigs[(user as SocketGuildUser).Guild.Id].VoiceChannelRoles
                    .HasElement(kvp => kvp.Key.Equals(after.VoiceChannel.Id), out chanrole))
                {
                    try
                    {
                        (user as IGuildUser).AddRoleAsync((user as SocketGuildUser).Guild.Roles.
                            FirstOrDefault(r => r.Id == chanrole.Value));
                    }
                    catch (Exception e)
                    {
                        GenericBot.Logger.LogErrorMessage(e.Message +"\n"  +e.StackTrace);
                    }
                }
            }
        }

        public static async Task UserLeft(SocketGuildUser user)
        {
            #region Logging

            var guildConfig = GenericBot.GuildConfigs[user.Guild.Id];


            if (guildConfig.UserLogChannelId == 0) return;

            try
            {
                string message = guildConfig.UserLeftMessage.Replace("{id}", user.Id.ToString()).Replace("{username}", user.ToString()).Replace("{mention}", user.Mention);
                if (guildConfig.UserLogTimestamp == true)
                {
                    message = $"`{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")}` {message}";
                }
                var logMessage = user.Guild.GetTextChannel(guildConfig.UserLogChannelId).SendMessageAsync(message).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            #endregion Logging
        }
    }
}
