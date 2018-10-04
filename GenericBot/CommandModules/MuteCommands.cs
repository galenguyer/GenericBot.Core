using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
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

            Command mutechannel = new Command("mutechannel");
            mutechannel.Description = "Prevent @-everyone from sending messages in a channel";
            mutechannel.RequiredPermission = Command.PermissionLevels.Moderator;
            mutechannel.ToExecute += async (client, msg, parameters) =>
            {
                var currentState = (msg.Channel as SocketGuildChannel).PermissionOverwrites
                .First(po => po.TargetId.Equals(msg.GetGuild().EveryoneRole.Id)).Permissions;

                if(currentState.SendMessages != PermValue.Deny) // If the channel isn't already muted
                {
                    GenericBot.GuildConfigs[msg.GetGuild().Id].ChannelOverrideDefaults.Override(msg.Channel.Id, currentState);
                    GenericBot.GuildConfigs[msg.GetGuild().Id].Save();

                    await (msg.Channel as SocketGuildChannel).AddPermissionOverwriteAsync(msg.GetGuild().EveryoneRole,
                        currentState.Modify(sendMessages: PermValue.Deny));

                    await msg.ReplyAsync("Sending messages to this channel has been temporarily disabled :mute:");
                }
                else // Channel is already muted
                {
                    await msg.ReplyAsync("This channel is already muted");
                }
            };

            MuteCommands.Add(mutechannel);

            Command unmutechannel = new Command("unmutechannel");
            unmutechannel.Description = "Reallow @-everyone to sending messages in a channel";
            unmutechannel.RequiredPermission = Command.PermissionLevels.Moderator;
            unmutechannel.ToExecute += async (client, msg, parameters) => 
            {
                var currentState = (msg.Channel as SocketGuildChannel).PermissionOverwrites
                .First(po => po.TargetId.Equals(msg.GetGuild().EveryoneRole.Id)).Permissions;

                if (currentState.SendMessages == PermValue.Deny)
                {
                    if (GenericBot.GuildConfigs[msg.GetGuild().Id].ChannelOverrideDefaults
                        .HasElement(e => e.Key.Equals(msg.Channel.Id), out KeyValuePair<ulong, Discord.OverwritePermissions> over))
                    {
                        await (msg.Channel as SocketGuildChannel).AddPermissionOverwriteAsync(msg.GetGuild().EveryoneRole,
                        over.Value);
                        await msg.ReplyAsync("Sending messages to this channel has been restored :loud_sound:");
                    }
                    else
                    {
                        await msg.ReplyAsync("This channel wasn't muted by the bot, so it can't be unmuted");
                    }
                }
                else
                {
                    await msg.ReplyAsync("This channel isn't muted");
                }
            };

            MuteCommands.Add(unmutechannel);

            return MuteCommands;
        }
    }
}
