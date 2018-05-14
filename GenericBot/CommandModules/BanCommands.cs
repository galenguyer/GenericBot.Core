using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Discord;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class BanCommands
    {
        public List<Command> GetBanCommands()
        {
            List<Command> banCommands = new List<Command>();

            Command ban = new Command("ban");
            ban.Description = "Ban a user from the server, whether or not they're on it";
            ban.Delete = false;
            ban.RequiredPermission = Command.PermissionLevels.Moderator;
            ban.Usage = $"{ban.Name} <user> <reason>";
            ban.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync($"You need to add some arguments. A user, perhaps?");
                    return;
                }

                var mentionedUsers = msg.GetMentionedUsers();
                if (!mentionedUsers.Any())
                {
                    await msg.ReplyAsync($"You have to give someone to ban");
                    return;
                }
                string reason = parameters.reJoin();
                reason = Regex.Replace(reason, @"^((<\@)?[0-9]{17,18}>?\s?)*", string.Empty);

                await msg.ReplyAsync(reason);

                var bans = msg.GetGuild().GetBansAsync().Result;

                foreach (var user in mentionedUsers)
                {
                    if (bans.Any(b => b.User.Id == user.Id))
                    {
                        await msg.ReplyAsync($"**`{user}` is already banned for `{bans.First(b => b.User.Id == user.Id).Reason}`**");
                    }
                    else
                    {
                        string banMessage = $"**Banned `{user}`(`{user.Id}`)";
                        if (string.IsNullOrEmpty(reason))
                            banMessage += $"** 👌";
                        else
                            banMessage += $"For `{reason}`** 👌";
                        await msg.GetGuild().AddBanAsync(user.Id);
                        await msg.ReplyAsync(banMessage);
                    }
                }
            };

            banCommands.Add(ban);

            return banCommands;
        }
    }
}
