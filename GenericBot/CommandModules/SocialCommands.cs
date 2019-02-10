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
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace GenericBot.CommandModules
{
    public class SocialCommands
    {
        public List<Command> GetSocialCommands()
        {
            List<Command> SocialCommands = new List<Command>();

            Command mock = new Command("mock");
            mock.Description = "MOcKinG sPoNgeBoB TeXt";
            mock.ToExecute += async (client, msg, parameters) =>
            {
                string rawMesage = CommandHandler.GetParameterString(msg);
                string mockedMessage = "";
                double rand = new Random().NextDouble();
                foreach(var c in rawMesage.ToLower())
                {
                    rand += new Random().NextDouble();
                    if (rand >= 1)
                    {
                        rand = new Random().NextDouble();
                        mockedMessage += char.ToUpper(c);
                    }
                    else
                        mockedMessage += c;
                }
                await msg.ReplyAsync(mockedMessage);
            };
            SocialCommands.Add(mock);

            Command star = new Command("star");
            star.ToExecute += async (client, msg, parameters) =>
            {
                string filename = "";
                if (parameters.Empty())
                {
                    var user = msg.Author;
                    using (WebClient webClient = new WebClient())
                    {
                        await webClient.DownloadFileTaskAsync(new Uri(user.GetAvatarUrl().Replace("size=128", "size=512")),
                            $"files/img/{user.AvatarId}.png");
                    }
                    filename = $"files/img/{user.AvatarId}.png";
                }
                else if (Uri.IsWellFormedUriString(parameters[0], UriKind.RelativeOrAbsolute) &&
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

                {
                    int targetWidth = 1242;
                    int targetHeight = 764; //height and width of the finished image
                    Image baseImage = Image.FromFile("files/img/staroranangel.png");
                    Image avatar = Image.FromFile(filename);

                    //be sure to use a pixelformat that supports transparency
                    using (var bitmap = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb))
                    {
                        using (var canvas = Graphics.FromImage(bitmap))
                        {
                            //this ensures that the backgroundcolor is transparent
                            canvas.Clear(Color.Transparent);

                            //this paints the frontimage with a offset at the given coordinates
                            canvas.DrawImage(avatar, 283, 228, 118 * avatar.Width / avatar.Height, 118);
                            canvas.DrawImage(avatar, 746, 250, 364 * avatar.Width / avatar.Height, 346);

                            //this selects the entire backimage and and paints
                            //it on the new image in the same size, so its not distorted.
                            canvas.DrawImage(baseImage, 0, 0, targetWidth, targetHeight);
                            canvas.Save();
                        }

                        bitmap.Save($"files/img/star_{msg.Id}.png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                    await Task.Delay(100);
                    await msg.Channel.SendFileAsync($"files/img/star_{msg.Id}.png");
                    baseImage.Dispose();
                    avatar.Dispose();
                    File.Delete(filename);
                    File.Delete($"files/img/star_{msg.Id}.png");
                }
            };

            SocialCommands.Add(star);

            Command respects = new Command("respects");
            respects.ToExecute += async (client, msg, parameters) =>
            {
                string filename = "";
                if (parameters.Empty())
                {
                    var user = msg.Author;
                    using (WebClient webClient = new WebClient())
                    {
                        await webClient.DownloadFileTaskAsync(new Uri(user.GetAvatarUrl().Replace("size=128", "size=512")),
                            $"files/img/{user.AvatarId}.png");
                    }
                    filename = $"files/img/{user.AvatarId}.png";
                }
                else if (Uri.IsWellFormedUriString(parameters[0], UriKind.RelativeOrAbsolute) &&
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

                {
                    int targetWidth = 1920;
                    int targetHeight = 1080; //height and width of the finished image
                    Image baseImage = Image.FromFile("files/img/respects.png");
                    Image avatar = Image.FromFile(filename);

                    //be sure to use a pixelformat that supports transparency
                    using (var bitmap = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb))
                    {
                        using (var canvas = Graphics.FromImage(bitmap))
                        {
                            //this ensures that the backgroundcolor is transparent
                            canvas.Clear(Color.Transparent);

                            //this paints the frontimage with a offset at the given coordinates
                            canvas.DrawImage(avatar, 537, 130, 235, 235);

                            //this selects the entire backimage and and paints
                            //it on the new image in the same size, so its not distorted.
                            canvas.DrawImage(baseImage, 0, 0, targetWidth, targetHeight);
                            canvas.Save();
                        }

                        bitmap.Save($"files/img/respects_{msg.Id}.png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                    await Task.Delay(100);
                    await msg.Channel.SendFileAsync($"files/img/respects_{msg.Id}.png");
                    baseImage.Dispose();
                    avatar.Dispose();
                    File.Delete(filename);
                    File.Delete($"files/img/respects_{msg.Id}.png");
                }
            };

            SocialCommands.Add(respects);

            Command box = new Command("box");
            box.Aliases = new List<string> { "boxer" };
            box.ToExecute += async (client, msg, parameters) =>
            {
                var workout = $"files/boxer/{msg.Id}";
                Directory.CreateDirectory(workout);
                var random = new Random();
                for (int i = 0; i < 8; i++)
                {
                    File.Copy($"files/boxer/boxer{random.Next(1, 4)}.png", $"{workout}/boxer{i}.png");
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process cmd = new Process();
                    cmd.StartInfo.FileName = "cmd.exe";
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.RedirectStandardOutput = true;
                    cmd.StartInfo.RedirectStandardError = true;
                    cmd.StartInfo.CreateNoWindow = true;
                    cmd.StartInfo.UseShellExecute = false;
                    cmd.Start();

                    cmd.StandardInput.WriteLine($"ffmpeg -t 2 -f image2 -framerate 2 -i {workout}/boxer%d.png -vf scale=300:-1 {workout}/boxer.gif");
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                    cmd.WaitForExit();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = "/bin/bash";
                    proc.StartInfo.Arguments = $"-c \"ffmpeg -t 2 -f image2 -framerate 2 -i {workout}/boxer%d.png -vf scale=300:-1 {workout}/boxer.gif\"";
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.Start();
                    proc.WaitForExit();
                }
                else
                {
                    await msg.Channel.SendMessageAsync("Unrecognized platform");
                }

                if (!File.Exists($"{workout}/boxer.gif"))
                {
                    await msg.Channel.SendMessageAsync("ffmpeg not installed. Contact the bot maintainer to solve this.");
                }
                else
                {
                    await msg.Channel.SendFileAsync($"{workout}/boxer.gif");
                    Directory.Delete(workout, recursive: true);
                }

            };
            SocialCommands.Add(box);

            Command jeff = new Command("jeff");
            jeff.ToExecute += async (client, msg, parameters) =>
            {
                string filename = "";
                if (parameters.Empty())
                {
                    var user = msg.Author;
                    using (WebClient webClient = new WebClient())
                    {
                        await webClient.DownloadFileTaskAsync(new Uri(user.GetAvatarUrl().Replace("size=128", "size=512")),
                            $"files/img/{user.AvatarId}.png");
                    }
                    filename = $"files/img/{user.AvatarId}.png";
                }
                else if (Uri.IsWellFormedUriString(parameters[0], UriKind.RelativeOrAbsolute) &&
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

            Command warm = new Command("warm");
            warm.ToExecute += async (client, msg, parameters) =>
            {
                string filename = "";
                if (parameters.Empty())
                {
                    var user = msg.Author;
                    using (WebClient webClient = new WebClient())
                    {
                        await webClient.DownloadFileTaskAsync(new Uri(user.GetAvatarUrl(size: 512)),
                            $"files/img/{user.AvatarId}.png");
                    }
                    filename = $"files/img/{user.AvatarId}.png";
                }
                else if (Uri.IsWellFormedUriString(parameters[0], UriKind.RelativeOrAbsolute) &&
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

                {
                    int targetWidth = 596;
                    int targetHeight = 684; //height and width of the finished image
                    Image baseImage = Image.FromFile("files/img/warm.png");
                    Image avatar = Image.FromFile(filename);

                    //be sure to use a pixelformat that supports transparency
                    using (var bitmap = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb))
                    {
                        using (var canvas = Graphics.FromImage(bitmap))
                        {
                            //this ensures that the backgroundcolor is transparent
                            canvas.Clear(Color.White);

                            //this paints the frontimage with a offset at the given coordinates
                            canvas.DrawImage(avatar, 20, 466, 218, 218);

                            //this selects the entire backimage and and paints
                            //it on the new image in the same size, so its not distorted.
                            canvas.DrawImage(baseImage, 0, 0, targetWidth, targetHeight);
                            canvas.Save();
                        }

                        bitmap.Save($"files/img/warm{msg.Id}.png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                    await Task.Delay(100);
                    await msg.Channel.SendFileAsync($"files/img/warm{msg.Id}.png");
                    baseImage.Dispose();
                    avatar.Dispose();
                    File.Delete(filename);
                    File.Delete($"files/img/warm{msg.Id}.png");
                }
            };

            SocialCommands.Add(warm);

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
                    await Task.Delay(15000);
                    await msg.DeleteAsync();
                    await resMessge.DeleteAsync();
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
                catch
                {
                    await msg.Channel.SendMessageAsync("", embed: new EmbedBuilder()
                        .WithColor(255, 0, 0)
                        .WithDescription("Invalid invite").Build());
                }
            };

            SocialCommands.Add(checkinvite);

            Command generateInvite = new Command("generateInvite");
            generateInvite.Description = "Generate a new invite with 1 use that lasts 24 hours";
            generateInvite.ToExecute += async (client, msg, parameters) =>
            {                                                              /*24h  uses temp  unique */
                var invite = msg.GetGuild().DefaultChannel.CreateInviteAsync(86400, 1, false, true).Result;
                await msg.ReplyAsync($"Here's your invite! It will last `24 hours` and has `1` use: {invite.Url}");
                if(msg.GetGuild().TextChannels.HasElement(c => c.Id == GenericBot.GuildConfigs[msg.GetGuild().Id].UserLogChannelId, out var channel))
                {
                    var emb = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder().WithName($"{msg.Author} ({msg.Author.Id})"))
                    .WithTitle("Created Invite")
                    .AddField(new EmbedFieldBuilder().WithName("channel").WithValue(invite.ChannelName))
                    .AddField(new EmbedFieldBuilder().WithName("invite").WithValue(invite.Url))
                    .WithCurrentTimestamp()
                    .WithColor(5126509);

                    await channel.SendMessageAsync("", embed: emb.Build());
                }
            };

            SocialCommands.Add(generateInvite);

            Command hug = new Command("hug");
            hug.Delete = true;
            hug.Usage = "hug <?user>";
            hug.ToExecute += async (client, msg, parameters) =>
            {
                if (msg.MentionedUsers.Any())
                {
                    await msg.ReplyAsync($"_\\*{msg.Author.Mention} hugs {msg.MentionedUsers.Select(u => u.Mention).ToList().SumAnd()}*_");
                }
                else
                {
                    await msg.ReplyAsync($"_\\*hugs {msg.Author.Mention}*_");
                }
            };

            SocialCommands.Add(hug);

            Command findGroup = new Command("findgroup");
            findGroup.Description = "Find everyone who's Playing status is a certain game";
            findGroup.Usage = "findgroup <game>";
            findGroup.ToExecute += async (client, msg, parameters) =>
            {
                await msg.GetGuild().DownloadUsersAsync();
                string gameToSearch = parameters.reJoin().ToLower();
                if (parameters.Empty())
                {
                    await msg.ReplyAsync("Please specify a game to search for");
                    return;
                }
                if(parameters.reJoin("").Length < 3)
                {
                    await msg.ReplyAsync("Sorry, that's too short to search for");
                    return;
                }
                string users = "";
                int count = 0;

                foreach(var member in msg.GetGuild().Users.Where(m => m.Activity != null && m.Activity.Type == ActivityType.Playing && m.Activity.Name.ToLower().Contains(gameToSearch)))
                {
                    users += $"{member.GetDisplayName().Escape()} (`{member}`)\n";
                    count++;
                }

                users = $"{count} users playing {parameters.reJoin()}\n{users}";

                foreach (var str in users.SplitSafe('\n'))
                {
                    await msg.ReplyAsync(str);
                }
            };

            SocialCommands.Add(findGroup);

            return SocialCommands;
        }
    }
}
