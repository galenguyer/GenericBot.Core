using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class MuteCommands
    {
        public List<Command> GetMuteCommands()
        {
            List<Command> MuteCommands = new List<Command>();

            Command channelmute = new Command("channelmute");
            channelmute.Description = "Mute a user from a specific channel for a period of time";
            channelmute.Usage = "channelmute <user> <channel> <time> [+reactions]";
            channelmute.RequiredPermission = Command.PermissionLevels.Moderator;
            channelmute.ToExecute += async (client, msg, parameters) =>
            {

                bool newMute = true;
                if (parameters.Empty())
                {
                    await msg.ReplyAsync("You have to specify more details");
                    return;
                }
                var users = msg.GetMentionedUsers();
                if (!users.Any())
                {
                    await msg.ReplyAsync("You have to select a user");
                    return;
                }
                if (!msg.MentionedChannels.Any())
                {
                    await msg.ReplyAsync($"You have to select a channel like this: <#{msg.Channel.Id}>");
                    return;
                }
                var channel = msg.MentionedChannels.First();
                newMute = !(GenericBot.GuildConfigs[msg.GetGuild().Id].ChannelMutes
                    .Any(m => m.UserId == users.First().Id && m.ChannelId == channel.Id));

                int time;
                if (int.TryParse(parameters[2], out time) || !newMute)
                {

                }
                else
                {
                    await msg.ReplyAsync("You have to specify a time in days");
                    return;
                }

                OverwritePermissions overrides;
                var react = "";
                if (parameters.Last().ToLower().Equals("+reactions"))
                {
                    overrides = new OverwritePermissions(sendMessages: PermValue.Deny, addReactions: PermValue.Deny);
                    react += " and reacting";
                }
                else overrides = new OverwritePermissions(sendMessages: PermValue.Deny);

                await channel.AddPermissionOverwriteAsync(users.First(), overrides);

                if (newMute)
                {
                    GenericBot.GuildConfigs[msg.GetGuild().Id].ChannelMutes.Add(new ChannelMute()
                    {
                        ChannelId = channel.Id,
                        UserId = users.First().Id,
                        RemovealTime = time.Equals(0) ? DateTime.MaxValue : DateTime.UtcNow + TimeSpan.FromDays(time)
                    });
                    GenericBot.GuildConfigs[msg.GetGuild().Id].Save();
                    string t = time.Equals(0) ? "forever" : $"for {time} days";
                    await msg.ReplyAsync(
                        $"Stopped {users.First().Mention} from sending messages{react} in <#{channel.Id}> {t}");
                }
                else
                {
                    if (parameters.Last().ToLower().Equals("+reactions"))
                    {
                        await msg.ReplyAsync($"Stopped {users.First().Mention} from reacting in <#{channel.Id}>");
                    }
                    else
                    {
                        await msg.ReplyAsync("Nothing was changed");
                    }
                }
            };

            MuteCommands.Add(channelmute);

            return MuteCommands;
        }
    }
}
