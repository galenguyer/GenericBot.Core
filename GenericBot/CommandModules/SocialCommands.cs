using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GenericBot.Entities;
using Newtonsoft.Json;

namespace GenericBot.CommandModules
{
    public class SocialCommands
    {
        public List<Command> GetSocialCommands()
        {
            List<Command> SocialCommands = new List<Command>();

            Command giveaway = new Command("giveaway");
            giveaway.Usage = "giveaway <start|close|roll>";
            giveaway.Description = "Start or end a giveaway";
            giveaway.RequiredPermission = Command.PermissionLevels.Moderator;
            giveaway.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync(
                        $"You have to tell me to do something. _\\*(Try `{GenericBot.GuildConfigs[msg.GetGuild().Id].Prefix}help giveaway)*_ for some options");
                    return;
                }
                string op = parameters[0].ToLower();
                var guildConfig = GenericBot.GuildConfigs[msg.GetGuild().Id];
                if (op.Equals("start"))
                {
                    if (guildConfig.Giveaway == null || !guildConfig.Giveaway.Open)
                    {
                        guildConfig.Giveaway = new Giveaway();
                        await msg.ReplyAsync($"A new giveaway has been created!");
                    }
                    else
                    {
                        await msg.ReplyAsync(
                            $"There is already an open giveaway! You have to close it before you can open a new one.");
                    }
                }
                else if (op.Equals("close"))
                {
                    if (guildConfig.Giveaway == null || !guildConfig.Giveaway.Open)
                    {
                        await msg.ReplyAsync($"There's no open giveaway.");
                    }
                    else
                    {
                        guildConfig.Giveaway.Open = false;
                        await msg.ReplyAsync($"Giveaway closed! {guildConfig.Giveaway.Hopefuls.Count} people entered.");
                    }
                }
                else if (op.Equals("roll"))
                {
                    if (guildConfig.Giveaway == null)
                    {
                        await msg.ReplyAsync($"There's no existing giveaway.");
                    }
                    else if (guildConfig.Giveaway.Open)
                    {
                        await msg.ReplyAsync("You have to close the giveaway first!");
                    }
                    else
                    {
                        await msg.ReplyAsync(
                            $"<@{guildConfig.Giveaway.Hopefuls.GetRandomItem()}> has won... something!");
                    }
                }
                else
                {
                    await msg.ReplyAsync($"That's not a valid option");
                }
                guildConfig.Save();
            };

            SocialCommands.Add(giveaway);

            Command g = new Command("g");
            g.Description = "Enter into the active giveaway";
            g.ToExecute += async (client, msg, parameters) =>
            {
                var guildConfig = GenericBot.GuildConfigs[msg.GetGuild().Id];
                {
                    RestUserMessage resMessge;
                    if (guildConfig.Giveaway == null || !guildConfig.Giveaway.Open)
                    {
                        resMessge = msg.ReplyAsync($"There's no open giveaway.").Result;
                    }
                    else
                    {
                        var guildConfigGiveaway = guildConfig.Giveaway;
                        if (guildConfigGiveaway.Hopefuls.Contains(msg.Author.Id))
                        {
                            resMessge = msg.ReplyAsync($"You're already in this giveaway.").Result;
                        }
                        else
                        {
                            guildConfigGiveaway.Hopefuls.Add(msg.Author.Id);
                            resMessge = msg.ReplyAsync($"You're in, {msg.Author.Mention}. Good luck!").Result;
                        }
                    }
                    GenericBot.QueueMessagesForDelete(new List<IMessage> {msg, resMessge});
                }
                guildConfig.Save();
            };

            SocialCommands.Add(g);

            Command checkinvite = new Command("checkinvite");
            checkinvite.Aliases = new List<string> {"invite"};
            checkinvite.Description = "Check the information of a discord invite";
            checkinvite.Usage = "checkinvite <code>";
            checkinvite.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync($"You need to give me a code to look at!");
                    return;
                }
                var inviteCode = parameters.Last().Split("/").Last();
                try
                {
                    var invite = client.GetInviteAsync(inviteCode).Result;
                    if (invite.Equals(null))
                    {
                        await msg.Channel.SendMessageAsync("", embed: new EmbedBuilder()
                            .WithColor(255, 0, 0)
                            .WithDescription("Invalid invite"));
                    }

                    var embedBuilder = new EmbedBuilder()
                        .WithColor(0, 255, 0)
                        .WithTitle("Valid Invite")
                        .WithUrl($"https://discord.gg/{invite.Code}")
                        .AddField(new EmbedFieldBuilder().WithName("Guild Name").WithValue(invite.GuildName)
                            .WithIsInline(true))
                        .AddField(new EmbedFieldBuilder().WithName("_ _").WithValue("_ _").WithIsInline(true))
                        .AddField(new EmbedFieldBuilder().WithName("Channel Name").WithValue(invite.ChannelName)
                            .WithIsInline(true))
                        .AddField(new EmbedFieldBuilder().WithName("Guild Id").WithValue(invite.GuildId)
                            .WithIsInline(true))
                        .AddField(new EmbedFieldBuilder().WithName("_ _").WithValue("_ _").WithIsInline(true))
                        .AddField(new EmbedFieldBuilder().WithName("Channel Id").WithValue(invite.ChannelId)
                            .WithIsInline(true))
                        .WithCurrentTimestamp();

                    await msg.Channel.SendMessageAsync("", embed: embedBuilder.Build());
                }
                catch (Exception e)
                {
                    await msg.Channel.SendMessageAsync("", embed: new EmbedBuilder()
                        .WithColor(255, 0, 0)
                        .WithDescription("Invalid invite"));
                }
            };

            SocialCommands.Add(checkinvite);

            Command poll = new Command("poll");
            poll.Description =
                "Creates, adds an option to, or closes a poll. -multi flag enables multiple-choice poll.";
            poll.Usage = "poll <start [-multi] <text>|add <option>|remove <option>|merge <to> <from>|close>";
            poll.Delete = true;
            poll.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync("You need to give me a command!");
                    return;
                }

                if (parameters[0] == "start" || parameters[0] == "new" || parameters[0] == "create")
                {
                    bool multi = false;
                    if (parameters[1] == "-multi")
                    {
                        multi = true;
                        parameters.Remove("-multi");
                    }

                    Poll activePoll = GenericBot.GuildConfigs[msg.GetGuild().Id].Polls
                        .GetValueOrDefault(msg.Channel.Id, null);
                    if (activePoll != null)
                    {
                        await msg.ReplyAsync(
                            "There is already a poll active in this channel. If you are the creator of that poll, or a moderator, you can close it.");
                        return;
                    }

                    Poll newPoll = new Poll();
                    newPoll.Creator = msg.Author.Id;
                    newPoll.MultipleChoice = multi;
                    newPoll.Text = String.Join(" ", parameters.GetRange(1, parameters.Count - 1));

                    var m = await msg.ReplyAsync($"...");
                    newPoll.MessageId = m.Id;
                    GenericBot.GuildConfigs[msg.GetGuild().Id].Polls[msg.Channel.Id] = newPoll;
                    GenericBot.GuildConfigs[msg.GetGuild().Id].Save();

                    UpdatePollMessage(msg.Channel, newPoll);
                }
                else if (parameters[0] == "add")
                {
                    Poll activePoll = GenericBot.GuildConfigs[msg.GetGuild().Id].Polls
                        .GetValueOrDefault(msg.Channel.Id, null);
                    if (activePoll == null)
                    {
                        await msg.ReplyAsync("There is no active poll in this channel.");
                        return;
                    }

                    if (msg.Author.Id == activePoll.Creator || poll.GetPermissions(msg.Author, msg.GetGuild().Id) >
                        Command.PermissionLevels.Moderator)
                    {
                        string optText = String.Join(" ", parameters.GetRange(1, parameters.Count - 1));
                        PollOption opt = new PollOption();
                        opt.Text = optText;
                        activePoll.Options.Add(opt);
                        GenericBot.GuildConfigs[msg.GetGuild().Id].Save();

                        await msg.ReplyAsync($"Added option **{optText}** to poll!");
                        UpdatePollMessage(msg.Channel, activePoll);
                        return;
                    }
                    else
                    {
                        await msg.ReplyAsync("Only the poll creator or a moderator can add an option to a poll.");
                        return;
                    }
                }
                else if (parameters[0] == "remove" || parameters[0] == "delete")
                {
                    if (parameters.Count < 2)
                    {
                        await msg.ReplyAsync("You must pass an option number to remove.");
                        return;
                    }

                    Poll activePoll = GenericBot.GuildConfigs[msg.GetGuild().Id].Polls
                        .GetValueOrDefault(msg.Channel.Id, null);
                    if (activePoll == null)
                    {
                        await msg.ReplyAsync("There is no active poll in this channel.");
                        return;
                    }

                    if (msg.Author.Id == activePoll.Creator || poll.GetPermissions(msg.Author, msg.GetGuild().Id) >
                        Command.PermissionLevels.Moderator)
                    {
                        int optNum;
                        if (!int.TryParse(parameters[1], out optNum))
                        {
                            await msg.ReplyAsync("That's not a number!");
                            return;
                        }
                        optNum--;

                        if (optNum < 0 || optNum >= activePoll.Options.Count)
                        {
                            await msg.ReplyAsync("That's not a poll option!");
                            return;
                        }

                        activePoll.Options.RemoveAt(optNum);
                        UpdatePollMessage(msg.Channel, activePoll);
                        GenericBot.GuildConfigs[msg.GetGuild().Id].Save();
                    }
                    else
                    {
                        await msg.ReplyAsync("Only the poll creator or a moderator can remove an option.");
                        return;
                    }
                }
                else if (parameters[0] == "merge")
                {
                    if (parameters.Count < 3)
                    {
                        await msg.ReplyAsync("You must pass two option numbers to merge.");
                        return;
                    }

                    Poll activePoll = GenericBot.GuildConfigs[msg.GetGuild().Id].Polls
                        .GetValueOrDefault(msg.Channel.Id, null);
                    if (activePoll == null)
                    {
                        await msg.ReplyAsync("There is no active poll in this channel.");
                        return;
                    }

                    if (msg.Author.Id == activePoll.Creator || poll.GetPermissions(msg.Author, msg.GetGuild().Id) >
                        Command.PermissionLevels.Moderator)
                    {
                        if (!int.TryParse(parameters[1], out int optNum1))
                        {
                            await msg.ReplyAsync("That's not a number!");
                            return;
                        }
                        optNum1--;

                        if (optNum1 < 0 || optNum1 >= activePoll.Options.Count)
                        {
                            await msg.ReplyAsync("That's not a poll option!");
                            return;
                        }

                        if (!int.TryParse(parameters[2], out var optNum2))
                        {
                            await msg.ReplyAsync("That's not a number!");
                            return;
                        }
                        optNum2--;

                        if (optNum2 < 0 || optNum2 >= activePoll.Options.Count)
                        {
                            await msg.ReplyAsync("That's not a poll option!");
                            return;
                        }

                        if (optNum1 == optNum2)
                        {
                            await msg.ReplyAsync("Cannot merge the same option!");
                            return;
                        }

                        activePoll.Options[optNum1].Voters.UnionWith(activePoll.Options[optNum2].Voters);
                        activePoll.Options.RemoveAt(optNum2);
                        UpdatePollMessage(msg.Channel, activePoll);
                        GenericBot.GuildConfigs[msg.GetGuild().Id].Save();
                    }
                    else
                    {
                        await msg.ReplyAsync("Only the poll creator or a moderator can merge options.");
                        return;
                    }
                }
                else if (parameters[0] == "close")
                {
                    Poll activePoll = GenericBot.GuildConfigs[msg.GetGuild().Id].Polls
                        .GetValueOrDefault(msg.Channel.Id, null);
                    if (activePoll == null)
                    {
                        await msg.ReplyAsync("There is no active poll in this channel.");
                        return;
                    }

                    if (msg.Author.Id == activePoll.Creator || poll.GetPermissions(msg.Author, msg.GetGuild().Id) >
                        Command.PermissionLevels.Moderator)
                    {
                        var eb = new EmbedBuilder().WithDescription("Poll has been closed.").WithTitle(activePoll.Text);

                        var ordered = activePoll.Options.OrderBy(p => -p.Voters.Count).ToList();
                        for (var i = 0; i < ordered.Count; i++)
                        {
                            var opt = ordered[i];
                            eb.AddField($"{i + 1}. {opt.Text}", $"({opt.Voters.Count} votes)");
                        }

                        await msg.Channel.SendMessageAsync("", embed: eb.Build());
                        GenericBot.GuildConfigs[msg.GetGuild().Id].Polls[msg.Channel.Id] = null;
                        GenericBot.GuildConfigs[msg.GetGuild().Id].Save();
                    }
                    else
                    {
                        await msg.ReplyAsync("Only the poll creator or a moderator can close a poll.");
                        return;
                    }
                }
            };

            SocialCommands.Add(poll);

            Command vote = new Command("vote");
            vote.Description = "Vote on an active poll";
            vote.Usage = "vote <number>";
            vote.ToExecute = async (client, msg, parameters) =>
            {
                Poll activePoll = GenericBot.GuildConfigs[msg.GetGuild().Id].Polls
                    .GetValueOrDefault(msg.Channel.Id, null);
                if (activePoll == null)
                {
                    await msg.ReplyAsync("There is no active poll in this channel.");
                    return;
                }

                if (parameters.Empty())
                {
                    var m = await msg.Channel.SendMessageAsync("", embed: CreatePollEmbed(activePoll));
                    GenericBot.QueueMessagesForDelete(new List<IMessage> {m});
                    return;
                }

                int optNum;
                if (!int.TryParse(parameters[0], out optNum))
                {
                    await msg.ReplyAsync("That's not a number!");
                    return;
                }
                optNum--;

                if (optNum < 0 || optNum > activePoll.Options.Count)
                {
                    await msg.ReplyAsync("That's not a poll option!");
                    return;
                }

                var voted = activePoll.GetVoted(msg.Author.Id);
                var option = activePoll.Options[optNum];
                activePoll.Vote(msg.Author.Id, optNum);

                IMessage resp = await msg.ReplyAsync($"**{msg.Author}** votes for **{option.Text}**");
                GenericBot.QueueMessagesForDelete(new List<IMessage> {resp});
                GenericBot.GuildConfigs[msg.GetGuild().Id].Save();

                UpdatePollMessage(msg.Channel, activePoll);
                await msg.DeleteAsync();
            };
            SocialCommands.Add(vote);

            return SocialCommands;
        }

        public Embed CreatePollEmbed(Poll poll)
        {
            var eb = new EmbedBuilder()
                .WithTitle(poll.Text)
                .WithDescription(
                    $"Vote using `{GenericBot.GlobalConfiguration.DefaultPrefix}vote <#>`.{(poll.MultipleChoice ? " You may vote for multiple options." : "")}");

            var ordered = poll.Options.OrderBy(p => -p.Voters.Count).ToList();
            for (var i = 0; i < ordered.Count; i++)
            {
                var opt = ordered[i];
                var idx = poll.Options.IndexOf(opt);
                eb.AddField($"{idx + 1}. {opt.Text}", $"({opt.Voters.Count} votes)");
            }
            return eb.Build();
        }

        public async void UpdatePollMessage(IMessageChannel chan, Poll poll)
        {
            IUserMessage msg = (IUserMessage) await chan.GetMessageAsync(poll.MessageId);
            await msg.ModifyAsync(x =>
            {
                x.Embed = CreatePollEmbed(poll);
                x.Content = "";
            });
        }
    }
}