using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericBot.CommandModules
{
    class BanModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command ban = new Command("ban");
            ban.Description = "Ban a user from the server, whether or not they're on it";
            ban.Delete = false;
            ban.RequiredPermission = Command.PermissionLevels.Moderator;
            ban.Usage = $"{ban.Name} <user> <time> <reason>";
            ban.ToExecute += async (context) =>
            {
                // Check for commands
                if (context.Parameters.IsEmpty())
                {
                    await context.Message.ReplyAsync($"You need to add some arguments. A user, perhaps?");
                    return;
                }

                // Parse out UserId
                ulong userId;
                if (!ulong.TryParse(context.Parameters[0].TrimStart('<', '@', '!').TrimEnd('>'), out userId))
                {
                    await context.Message.ReplyAsync("Try specifying someone to ban first");
                    return;
                }

                // Prevent banning me
                if (userId == Core.DiscordClient.GetApplicationInfoAsync().Result.Owner.Id)
                {
                    await context.Message.ReplyAsync("Haha lol no");
                    return;
                }

                // Remove UserId from param stack
                context.Parameters.RemoveAt(0);

                // Get the time to ban the user for
                var time = new DateTimeOffset();
                try
                {
                    if ((context.Parameters[0] == "0" || context.Parameters[0] == "0d"))
                        time = DateTimeOffset.MaxValue;
                    else
                        time = context.Parameters[0].ParseTimeString();
                    context.Parameters.RemoveAt(0);
                }
                // In case no time was specified
                catch (Exception ex)
                { time = DateTimeOffset.MaxValue; }
                string timeMessage = time == DateTimeOffset.MaxValue ? "permanently" : $"for `{(time.AddSeconds(1) - DateTimeOffset.UtcNow).FormatTimeString()}`";

                // Check if the user was already banned
                var bans = context.Guild.GetBansAsync().Result;
                if (bans.Any(b => b.User.Id == userId))
                {
                    await context.Message.ReplyAsync(
                        $"`{bans.First(b => b.User.Id == userId).User}` is already banned for `{bans.First(b => b.User.Id == userId).Reason}`");
                    return;
                }
                string reason = context.Parameters.IsEmpty() ? "No Reason Given" : context.Parameters.Rejoin();

                bool dmSuccess = true;
                string dmMessage = $"You have been banned from **{context.Guild.Name}** {timeMessage} " +
                $"for the following reason: \n\n{reason}\n\n";
                
                // Try to DM the user the message, set a flag and continue if they're blocking DMs
                try
                {
                    await context.Guild.GetUser(userId).GetOrCreateDMChannelAsync().Result
                        .SendMessageAsync(dmMessage);
                }
                catch
                {
                    dmSuccess = false;
                }

                try
                {
                    // We have to do some stuff to make the Discord audit log
                    // happy with what we send it
                    string auditReason = reason.Replace("\"", "'");
                    if (auditReason.Length > 256)
                    {
                        auditReason = auditReason.Substring(0, 250) + "...";
                    }
                    await context.Guild.AddBanAsync(userId, reason: auditReason);
                }
                catch
                {
                    await context.Message.ReplyAsync($"Could not ban the given user. Try checking role hierarchy and permissions");
                    return;
                }

                bans = context.Guild.GetBansAsync().Result;
                var user = bans.First(u => u.User.Id == userId).User;
                string banMessage = $"Banned `{user}` (`{user.Id}`)";
                if (string.IsNullOrEmpty(reason))
                    banMessage += $" 👌";
                else
                    banMessage += $" for `{reason}`";
                banMessage += $"{timeMessage} 👌";

                if (!dmSuccess) banMessage += "\nThe user could not be messaged";

                var builder = new EmbedBuilder()
                    .WithTitle("User Banned")
                    .WithDescription(banMessage)
                    .WithColor(new Color(0xFFFF00))
                    .WithFooter(footer => {
                        footer
                            .WithText($"By {context.Author} at {DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")} GMT");
                    })
                    .WithAuthor(author => {
                        author
                            .WithName(user.ToString())
                            .WithIconUrl(user.GetAvatarUrl());
                    });


                var guildconfig = Core.GetGuildConfig(context.Guild.Id);

                var bannedUser = Core.MongoEngine.GetUserFromGuild(user.Id, context.Guild.Id)
                    .AddWarning(
                        $"Banned {timeMessage} for `{reason}` (By `{context.Author}` At `{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")} GMT`)");
                Core.MongoEngine.SaveUserToGuild(bannedUser, context.Guild.Id);
                Core.MongoEngine.SaveBanToGuild(new GenericBan(user.Id, context.Guild.Id, reason, time), context.Guild.Id);

                // Send the ban logs
                await context.Channel.SendMessageAsync("", embed: builder.Build());
                if (guildconfig.LoggingChannelId != 0)
                {
                    await (Core.DiscordClient.GetChannel(guildconfig.LoggingChannelId) as SocketTextChannel)
                        .SendMessageAsync("", embed: builder.Build());
                }
            };
            commands.Add(ban);

            return commands;
        }
    }
}
