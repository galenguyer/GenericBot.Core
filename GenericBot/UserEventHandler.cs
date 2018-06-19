using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot
{
    public static class UserEventHandler
    {
        public static async Task UserUpdated(SocketGuildUser beforeUser, SocketGuildUser afterUser)
        {
            if (beforeUser.Username != afterUser.Username || beforeUser.Nickname != afterUser.Nickname)
            {
                var guildDb = new DBGuild(afterUser.Guild.Id);
                if (guildDb.Users.Any(u => u.ID.Equals(afterUser.Id))) // if already exists
                {
                    guildDb.Users.Find(u => u.ID.Equals(afterUser.Id)).AddUsername(beforeUser.Username);
                    guildDb.Users.Find(u => u.ID.Equals(afterUser.Id)).AddNickname(beforeUser);
                    guildDb.Users.Find(u => u.ID.Equals(afterUser.Id)).AddUsername(afterUser.Username);
                    guildDb.Users.Find(u => u.ID.Equals(afterUser.Id)).AddNickname(afterUser);
                }
                else
                {
                    guildDb.Users.Add(new DBUser(afterUser));
                }
                guildDb.Save();
            }
        }

        public static async Task UserJoined(SocketGuildUser user)
        {
            #region Database

            var guildDb = new DBGuild(user.Guild.Id);
            bool alreadyJoined = false;
            if (guildDb.Users.Any(u => u.ID.Equals(user.Id))) // if already exists
            {
                guildDb.Users.Find(u => u.ID.Equals(user.Id)).AddUsername(user.Username);
                alreadyJoined = true;
            }
            else
            {
                guildDb.Users.Add(new DBUser(user));
            }
            lock ("db")
            {
                guildDb.Save();
            }

            #endregion Databasae

            #region Logging

            var guildConfig = GenericBot.GuildConfigs[user.Guild.Id];

            if (!(guildConfig.VerifiedRole == 0 || (string.IsNullOrEmpty(guildConfig.VerifiedMessage) || guildConfig.VerifiedMessage.Split().Length < 32 || !user.Guild.Roles.Any(r => r.Id == guildConfig.VerifiedRole))))
            {
                string vMessage = $"Hey {user.Username}! To get verified on **{user.Guild.Name}** reply to this message with the hidden code in the message below\n\n"
                                 + GenericBot.GuildConfigs[user.Guild.Id].VerifiedMessage;

                string verificationMessage =
                    VerificationEngine.InsertCodeInMessage(vMessage, VerificationEngine.GetVerificationCode(user.Id, user.Guild.Id));

                try
                {
                    await user.GetOrCreateDMChannelAsync().Result.SendMessageAsync(verificationMessage);
                }
                catch (Exception ex)
                {
                }
            }
            if (guildConfig.ProbablyMutedUsers.Contains(user.Id))
            {
                try { user.AddRoleAsync(user.Guild.GetRole(guildConfig.MutedRoleId)); }
                catch { }
            }

            if (guildConfig.UserLogChannelId == 0) return;
            
            EmbedBuilder log = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder().WithName("User Joined")
                    .WithIconUrl(user.GetAvatarUrl()).WithUrl(user.GetAvatarUrl()))
                .WithColor(114, 137, 218)
                .AddField(new EmbedFieldBuilder().WithName("Username").WithValue(user.ToString()).WithIsInline(true))
                .AddField(new EmbedFieldBuilder().WithName("UserId").WithValue(user.Id).WithIsInline(true))
                .AddField(new EmbedFieldBuilder().WithName("Mention").WithValue(user.Mention).WithIsInline(true))
                .AddField(new EmbedFieldBuilder().WithName("User Number").WithValue(user.Guild.MemberCount + (!alreadyJoined ? " (New Member)" : "")).WithIsInline(true))
                .AddField(new EmbedFieldBuilder().WithName("Database Number").WithValue(guildDb.Users.Count + (alreadyJoined ? " (Previous Member)" : "" )).WithIsInline(true))
                .WithFooter($"{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")} GMT");

            if ((DateTimeOffset.Now - user.CreatedAt).TotalDays < 7)
            {
                log.AddField(new EmbedFieldBuilder().WithName("New User")
                    .WithValue($"Account made {(DateTimeOffset.Now - user.CreatedAt).Nice()} ago").WithIsInline(true));
            }

            try
            {
                DBUser usr = guildDb.Users.First(u => u.ID.Equals(user.Id));

                if (!usr.Warnings.Empty())
                {
                    string warns = "";
                    for(int i = 0; i < usr.Warnings.Count; i++)
                    {
                        warns += $"{i + 1}: {usr.Warnings.ElementAt(i)}\n";
                    }
                    log.AddField(new EmbedFieldBuilder().WithName($"{usr.Warnings.Count} Warnings")
                        .WithValue(warns));
                }
            }
            catch
            {
            }

            await user.Guild.GetTextChannel(guildConfig.UserLogChannelId).SendMessageAsync("", embed: log.Build());

            #endregion Logging
        }

        public static async Task UserLeft(SocketGuildUser user)
        {
            #region Logging

            var guildConfig = GenericBot.GuildConfigs[user.Guild.Id];

            if (guildConfig.UserLogChannelId == 0) return;

            EmbedBuilder log = new EmbedBuilder()
				.WithAuthor(new EmbedAuthorBuilder().WithName("User Left")
                    .WithIconUrl(user.GetAvatarUrl()).WithUrl(user.GetAvatarUrl()))
                .WithColor(156, 39, 176)
				.AddField(new EmbedFieldBuilder().WithName("Username").WithValue(user.ToString()).WithIsInline(true))
                .AddField(new EmbedFieldBuilder().WithName("UserId").WithValue(user.Id).WithIsInline(true))
                .AddField(new EmbedFieldBuilder().WithName("Mention").WithValue(user.Mention).WithIsInline(true))
				.WithFooter($"{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")} GMT");

            await user.Guild.GetTextChannel(guildConfig.UserLogChannelId).SendMessageAsync("", embed: log.Build());

            #endregion Logging
        }
    }
}
