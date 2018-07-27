using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Rest;
using GenericBot.Entities;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;

namespace GenericBot.CommandModules
{
    public class SocialCommands
    {
        public List<Command> GetSocialCommands()
        {
            List<Command> SocialCommands = new List<Command>();

            Command time = new Command("time");
            time.Description = "Get the current time with different timezone support";
            time.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    var currTime = DateTimeOffset.UtcNow.TimeOfDay;

                    bool pm = currTime.Hours >= 12;

                    int hour = currTime.Hours;
                    string minutes = "00" + currTime.Minutes;
                    if (pm)
                    {
                        hour = currTime.Hours - 12;
                    }

                    while (minutes.Length > 2)
                    {
                        minutes = minutes.Substring(1, 3);
                    }


                    string sTime = $"{hour}:{("00" + currTime.Minutes)} ";
                    if (pm) sTime += "PM";
                    else sTime += "AM";
                    await msg.ReplyAsync($"Current GMT time is `{sTime}`");
                    return;
                }
                try
                {
                    Dictionary<string, string> _timeZones = new Dictionary<string, string>() { { "ACDT", "+10:30" }, { "ACST", "+09:30" }, { "ADT", "-03:00" }, { "AEDT", "+11:00" }, { "AEST", "+10:00" }, { "AHDT", "-09:00" }, { "AHST", "-10:00" }, { "AST", "-04:00" }, { "AT", "-02:00" }, { "AWDT", "+09:00" }, { "AWST", "+08:00" }, { "BAT", "+03:00" }, { "BDST", "+02:00" }, { "BET", "-11:00" }, { "BST", "01:00" }, { "BT", "+03:00" }, { "BZT2", "-03:00" }, { "CADT", "+10:30" }, { "CAST", "+09:30" }, { "CAT", "-10:00" }, { "CCT", "+08:00" }, { "CDT", "-05:00" }, { "CED", "+02:00" }, { "CET", "+01:00" }, { "CEST", "+02:00" }, { "CST", "-06:00" }, { "EAST", "+10:00" }, { "EDT", "-04:00" }, { "EED", "+03:00" }, { "EET", "+02:00" }, { "EEST", "+03:00" }, { "EST", "-05:00" }, { "FST", "+02:00" }, { "FWT", "+01:00" }, { "GMT", "+00:00" }, { "GST", "+10:00" }, { "HDT", "-09:00" }, { "HST", "-10:00" }, { "IDLE", "+12:00" }, { "IDLW", "-12:00" }, { "IST", "+05:30" }, { "IT", "+03:30" }, { "JST", "+09:00" }, { "JT", "+07:00" }, { "MDT", "-06:00" }, { "MED", "+02:00" }, { "MET", "+01:00" }, { "MEST", "+02:00" }, { "MEWT", "+01:00" }, { "MST", "-07:00" }, { "MT", "+08:00" }, { "NDT", "-02:30" }, { "NFT", "-03:30" }, { "NT", "-11:00" }, { "NST", "+06:30" }, { "NZ", "+11:00" }, { "NZST", "+12:00" }, { "NZDT", "+13:00" }, { "NZT", "+12:00" }, { "PDT", "-07:00" }, { "PST", "-08:00" }, { "ROK", "+09:00" }, { "SAD", "+10:00" }, { "SAST", "+09:00" }, { "SAT", "+09:00" }, { "SDT", "+10:00" }, { "SST", "+02:00" }, { "SWT", "+01:00" }, { "USZ3", "+04:00" }, { "USZ4", "+05:00" }, { "USZ5", "+06:00" }, { "USZ6", "+07:00" }, { "UT", "-00:00" }, { "UTC", "-00:00" }, { "UZ10", "+11:00" }, { "WAT", "-01:00" }, { "WET", "-00:00" }, { "WST", "+08:00" }, { "YDT", "-08:00" }, { "YST", "-09:00" }, { "ZP4", "+04:00" }, { "ZP5", "+05:00" }, { "ZP6", "+06:00" } };
                    var currTime = DateTimeOffset.UtcNow.TimeOfDay + TimeSpan.Parse(_timeZones[parameters[0].ToUpper()]);

                    bool pm = currTime.Hours >= 12;

                    int hour = currTime.Hours;
                    if (pm)
                    {
                        hour = currTime.Hours - 12;
                    }

                    string sTime = $"{hour}:{currTime.Minutes} ";
                    if (pm) sTime += "PM";
                    else sTime += "AM";

                    await msg.ReplyAsync($"Current {parameters[0].ToUpper()} time is `{sTime}`");
                }
                catch (Exception ex)
                {
                    await msg.ReplyAsync($"An error occured: `{ex.Message}`");
                    if (msg.Author.Id == 169918990313848832) await msg.ReplyAsync($"```\n{ex.StackTrace}\n```");
                }
            };

            SocialCommands.Add(time);

            Command jeff = new Command("jeff");
            jeff.ToExecute += async (client, msg, parameters) =>
            {
                string filename = "";
                if (Uri.IsWellFormedUriString(parameters[0], UriKind.RelativeOrAbsolute) &&
                                         (parameters[0].EndsWith(".png") || parameters[0].EndsWith(".jpg") ||
                                          parameters[0].EndsWith("jpeg") || parameters[0].EndsWith(".gif")))
                {
                    filename = $"files/img/{msg.Id}.{parameters.reJoin().Split('.').Last()}";
                    using (WebClient webclient = new WebClient())
                    {
                        await webclient.DownloadFileTaskAsync(new Uri(parameters.reJoin()), filename);
                    }
                }
                else if (msg.GetMentionedUsers().Any())
                {
                    var user = msg.GetMentionedUsers().First();
                    using (WebClient webClient = new WebClient())
                    {
                        await webClient.DownloadFileTaskAsync(new Uri(user.GetAvatarUrl().Replace("size=128", "size=512")),
                            $"files/img/{user.AvatarId}.png");
                    }
                    filename = $"files/img/{user.AvatarId}.png";
                }
                else
                {
                    var user = msg.Author;
                    using (WebClient webClient = new WebClient())
                    {
                        await webClient.DownloadFileTaskAsync(new Uri(user.GetAvatarUrl().Replace("size=128", "size=512")),
                            $"files/img/{user.AvatarId}.png");
                    }
                    filename = $"files/img/{user.AvatarId}.png";
                }

                {
                    int targetWidth = 1278;
                    int targetHeight = 717; //height and width of the finished image
                    Image baseImage = Image.FromFile("files/img/jeff.png");
                    Image avatar = Image.FromFile(filename);

                    //be sure to use a pixelformat that supports transparency
                    using (var bitmap = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb))
                    {
                        using (var canvas = Graphics.FromImage(bitmap))
                        {
                            //this ensures that the backgroundcolor is transparent
                            canvas.Clear(Color.White);

                            //this paints the frontimage with a offset at the given coordinates
                            canvas.DrawImage(avatar, 523, 92, 269, 269);

                            //this selects the entire backimage and and paints
                            //it on the new image in the same size, so its not distorted.
                            canvas.DrawImage(baseImage, 0, 0, targetWidth, targetHeight);
                            canvas.Save();
                        }

                        bitmap.Save($"files/img/jeff_{msg.Id}.png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                    await Task.Delay(100);
                    await msg.Channel.SendFileAsync($"files/img/jeff_{msg.Id}.png");
                    baseImage.Dispose();
                    avatar.Dispose();
                    File.Delete(filename);
                    File.Delete($"files/img/jeff_{msg.Id}.png");
                }
            };

            SocialCommands.Add(jeff);

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
                    GenericBot.QueueMessagesForDelete(new List<IMessage> { msg, resMessge });
                }
                guildConfig.Save();
            };

            SocialCommands.Add(g);

            Command checkinvite = new Command("checkinvite");
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
                            .WithDescription("Invalid invite").Build());
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
                        .WithDescription("Invalid invite").Build());
                }
            };

            SocialCommands.Add(checkinvite);

            return SocialCommands;
        }
    }
}
