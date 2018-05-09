using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class CardCommands
    {
        public List<Command> GetCardCommands()
        {
            List<Command> cardCommands = new List<Command>();

            Command userInfo = new Command("userInfo");
            userInfo.Description = "Get information about a mentioned user";
			userInfo.RequiredPermission = Command.PermissionLevels.Moderator;
            userInfo.SendTyping = false;
            userInfo.ToExecute += async (client, msg, parameters) =>
            {
                await msg.GetGuild().DownloadUsersAsync();
                if (!msg.GetMentionedUsers().Any())
                {
                    await msg.ReplyAsync("No user found");
                    return;
                }
                SocketGuildUser user;
                ulong uid = msg.GetMentionedUsers().FirstOrDefault().Id;

                if (msg.GetGuild().Users.Any(u => u.Id == uid))
                {
                    user = msg.GetGuild().GetUser(uid);
                }
                else
                {
                    await msg.ReplyAsync("User not found");
                    return;
                }

                string nickname = msg.GetGuild().Users.All(u => u.Id != uid) || string.IsNullOrEmpty(user.Nickname) ? "None" : user.Nickname;
                string roles = "";
                foreach (var role in user.Roles.Where(r => !r.Name.Equals("@everyone")).OrderByDescending(r => r.Position))
                {
                    roles += $"`{role.Name}`, ";
                }

                string info = $"Username: `{user.ToString()}`\n";
                info += $"Nickname: `{nickname}`\n";
                info += $"User Id:  `{user.Id}`\n";
                info += $"Created At: `{string.Format("{0:yyyy-MM-dd HH\\:mm\\:ss zzzz}", user.CreatedAt.LocalDateTime)}GMT` " +
                        $"(about {(DateTime.UtcNow - user.CreatedAt).Days} days ago)\n";
                if (user.JoinedAt.HasValue)
                    info +=
                        $"Joined At: `{string.Format("{0:yyyy-MM-dd HH\\:mm\\:ss zzzz}", user.JoinedAt.Value.LocalDateTime)}GMT`" +
                        $"(about {(DateTime.UtcNow - user.JoinedAt.Value).Days} days ago)\n";
                info += $"Roles: {roles.Trim(' ', ',')}\n";
                info += $"Bot: `{user.IsBot}`\n";
                if(!string.IsNullOrEmpty(user.GetAvatarUrl()))
                    info += $"Avatar Url: <{user.GetAvatarUrl().Replace("size=128", "size=2048")}>";


                await msg.ReplyAsync(info);
            };

            cardCommands.Add(userInfo);

            Command serverInfo = new Command("guildInfo");
            serverInfo.Aliases = new List<string>{"serverinfo"};
            serverInfo.Description = "Show some information about the guild";
            serverInfo.ToExecute += async (client, msg, parameters) =>
            {
                var guild = msg.GetGuild();
                await guild.DownloadUsersAsync();
                var bans = guild.GetBansAsync().Result;
                string info = "";
                info += $"Guild Name: `{guild.Name}`\n";
                info += $"Guild Id: `{guild.Id}`\n";
                info += $"Owner: `{guild.Owner}` (`{guild.OwnerId}`)\n";
                info += $"User Count: `{guild.MemberCount}`\n";
                info += $"Created At: `{string.Format("{0:yyyy-MM-dd HH\\:mm\\:ss zzzz}", guild.CreatedAt.LocalDateTime)}GMT` " +
                        $"(about {(DateTime.UtcNow - guild.CreatedAt).Days} days ago)\n";
                info += $"Text Channels: `{guild.TextChannels.Count}`\n";
                info += $"Voice Channels: `{guild.VoiceChannels.Count}`\n";
                info += $"Voice Region: `{guild.VoiceRegionId}`\n";
                info += $"Roles: `{guild.Roles.Count}`\n";
                info += $"Verification Level: `{guild.VerificationLevel}`\n";
                info += $"Partnered: `{guild.Features.Any()}`\n";
                info += $"Bans: `{bans.Count}` (`{bans.Count(b => b.User.AvatarId == null && b.User.Username.StartsWith("Deleted User "))}` Accounts Deleted)";
                info += $"";
                info += $"";

                await msg.ReplyAsync(info);
            };

            cardCommands.Add(serverInfo);

            return cardCommands;
        }
    }
}
