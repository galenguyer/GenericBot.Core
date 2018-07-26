using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using Newtonsoft.Json;

namespace GenericBot.CommandModules
{
    public class TestCommands
    {
        public List<Command> GetTestCommands()
        {
            List<Command> TestCommands = new List<Command>();

            Command listEmotes = new Command("listemotes");
            listEmotes.RequiredPermission = Command.PermissionLevels.Admin;
            listEmotes.Delete = true;
            listEmotes.ToExecute += async (client, msg, parameters) =>
            {
                if (!msg.GetGuild().Emotes.Any())
                {
                    await msg.ReplyAsync($"`{msg.GetGuild().Name}` has no emotes");
                    return;
                }
                string emotes = $"`{msg.GetGuild().Name}` has `{msg.GetGuild().Emotes.Count}` emotes:";
                int i = 0;
                foreach (var emote in msg.GetGuild().Emotes)
                {
                    if (i % 3 == 0)
                    {
                        emotes += "\n";
                    }
                    emotes += $"<:{emote.Name}:{emote.Id}> `:{emote.Name}:`";
                    for (int c = emote.Name.Length + 2; c < 16; c++)
                    {
                        emotes += " ";
                    }
                    i++;
                }
                await msg.ReplyAsync(emotes);
            };

            TestCommands.Add(listEmotes);

            Command updateDB = new Command("updateDB");
            updateDB.Delete = false;
            updateDB.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            updateDB.ToExecute += async (client, msg, paramList) =>
            {
                await msg.GetGuild().DownloadUsersAsync();
                int newUsers = 0;
                int updatedUsers = 0;

                var db = new DBGuild(msg.GetGuild().Id);
                foreach (var user in msg.GetGuild().Users)
                {
                    if (!db.Users.Any(u => u.ID.Equals(user.Id)))
                    {
                        db.Users.Add(new DBUser(user));
                        newUsers++;
                    }
                    else
                    {
                        db.GetUser(user.Id).AddUsername(user.Username);
                        db.GetUser(user.Id).AddNickname(user);
                        updatedUsers++;
                    }
                }

                db.Save();
                await msg.ReplyAsync($"`{newUsers}` users added to database, `{updatedUsers}` updated");
            };

            TestCommands.Add(updateDB);

            Command IdInfo = new Command("idInfo");
            IdInfo.Aliases = new List<string> { "id" };
            IdInfo.Description = "Get information from a given ID";
            IdInfo.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync("No ID given");
                    return;
                }
                ulong id;
                if (ulong.TryParse(parameters[0], out id))
                {
                    ulong rawtime = id >> 22;
                    long epochtime = (long)rawtime + 1420070400000;
                    DateTimeOffset time = DateTimeOffset.FromUnixTimeMilliseconds(epochtime);
                    await msg.ReplyAsync($"ID: `{id}`\nDateTime: `{time.ToString(@"yyyy-MM-dd HH:mm:ss.fff tt")} GMT`");
                }
                else await msg.ReplyAsync("That's not a valid ID");
            };

            TestCommands.Add(IdInfo);

            Command DBStats = new Command("dbstats");
            DBStats.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            DBStats.ToExecute += async (client, msg, parameters) =>
            {
                Stopwatch stw = new Stopwatch();
                stw.Start();
                string info = "";

                var guildDb = new DBGuild(msg.GetGuild().Id);

                info += $"Registered Users: `{guildDb.Users.Count}`\n";

                int unc = 0, nnc = 0, wnc = 0, nuc = 0;
                foreach (var user in guildDb.Users)
                {
                    if (user.Usernames != null && user.Usernames.Any())
                        unc += user.Usernames.Count;
                    if (user.Nicknames != null && user.Nicknames.Any())
                        nnc += user.Nicknames.Count;
                    if (user.Warnings != null && user.Warnings.Any())
                    {
                        wnc += user.Warnings.Count;
                        nuc++;
                    }
                }

                info += $"Stored Usernames: `{unc}`\n";
                info += $"Stored Nicknames: `{nnc}`\n";
                info += $"Stored Warnings:  `{wnc}`\n";
                info += $"Users with Warnings:  `{nuc}`\n";

                info += $"Access time: `{stw.ElapsedMilliseconds}`ms\n";
                await msg.ReplyAsync(info);
            };

            TestCommands.Add(DBStats);

            Command addwarning = new Command("oldwarning");
            addwarning.Description += "Add a warning to the database without meta info";
            addwarning.Usage = "oldwarning <user> <warning>";
            addwarning.RequiredPermission = Command.PermissionLevels.Admin;
            ulong uid;
            addwarning.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync("You must specify a user");
                    return;
                }

                if (ulong.TryParse(parameters[0].TrimStart('<', '@', '!').TrimEnd('>'), out uid))
                {
                    parameters.RemoveAt(0);
                    string warning = parameters.reJoin();
                    var guildDb = new DBGuild(msg.GetGuild().Id);
                    if (guildDb.Users.Any(u => u.ID.Equals(uid))) // if already exists
                    {
                        guildDb.Users.Find(u => u.ID.Equals(uid)).AddWarning(warning);
                    }
                    else
                    {
                        guildDb.Users.Add(new DBUser { ID = uid, Warnings = new List<string> { warning } });
                    }
                    guildDb.Save();
                    await msg.ReplyAsync($"Added `{warning.Replace('`', '\'')}` to <@{uid}> (`{uid}`)");
                }
                else
                {
                    await msg.ReplyAsync("Could not find that user");
                }

            };

            TestCommands.Add(addwarning);

            Command archivePins = new Command("archivePins");
            archivePins.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            archivePins.ToExecute += async (client, msg, parameters) =>
            {
                var msgs = msg.Channel.GetPinnedMessagesAsync().Result.ToList();
                if (msgs.Any())
                {

                    msgs.Reverse();
                    string header = "<html><head><style>body {background-color: #36393e; color: #fff; font-family: \"Trebuchet MS\", Helvetica, sans-serif; font-size: small }server {font-size: 150%}channel {font-size: 130%}username {font-size: 100%}message {font-size: 80%}reqinf {font-size: 60%; color: grey;}</style></head>";
                    string server = $"<body> <i><server>{msg.GetGuild().Name}</server> in <channel>#{msg.Channel.Name}</channel></i><hr>";
                    string messages = "";
                    foreach (var m in msgs)
                    {
                        string mess = m.Content;
                        foreach (var u in m.MentionedUsers)
                        {
                            mess = mess.Replace($"<@!{u.Id}>", $"@{u.Username}");
                            mess = mess.Replace($"<@{u.Id}>", $"@{u.Username}");
                            mess = mess.Replace(u.Mention, $"@{u.Username}");
                        }
                        foreach (var c in m.MentionedChannelIds)
                        {
                            mess = mess.Replace($"<#{c}>", $"#{(client.GetChannel(c) as IMessageChannel).Name}");
                        }
                        foreach (var u in m.MentionedRoleIds)
                        {
                            mess = mess.Replace($"<@&u>", $"@{msg.GetGuild().GetRole(u).Name}");
                        }
                        messages += $"<username>{m.Author}</username><br><message>{mess}</message><hr>";
                    }
                    string footer = $"<br><br> <i><reqinf>Requested by <b>{msg.Author}</b></reqinf></i></body></html>";

                    File.WriteAllText($"files/{msg.Channel.Name}_pins.html", header + server + messages + footer, Encoding.UTF8);
                    await msg.Channel.SendFileAsync($"files/{msg.Channel.Name}_pins.html");
                    File.Delete($"files/{msg.Channel.Name}_pins.html");
                }
                else await msg.ReplyAsync($"This channel has no pinned messages!");
            };
            TestCommands.Add(archivePins);

            TestCommands.Add(DBStats);
            Command verify = new Command("verify");
            verify.RequiredPermission = Command.PermissionLevels.User;
            verify.ToExecute += async (client, msg, parameter) =>
            {
                List<SocketUser> users = new List<SocketUser>();
                var guildConfig = GenericBot.GuildConfigs[msg.GetGuild().Id];

                if (parameter.Empty())
                {
                    if ((msg.Author as SocketGuildUser).Roles.Any(r => r.Id == guildConfig.VerifiedRole))
                    {
                        await msg.ReplyAsync("You're already verified");
                    }
                    users.Add(msg.Author);
                }
                else
                {
                    foreach (var user in msg.GetMentionedUsers())
                    {
                        if ((user as SocketGuildUser).Roles.Any(r => r.Id == guildConfig.VerifiedRole))
                        {
                            await msg.ReplyAsync($"{user.Username} is already verified");
                        }
                        else
                        {
                            users.Add(user);
                        }
                    }
                }


                if (guildConfig.VerifiedRole == 0)
                {
                    await msg.ReplyAsync($"Verification is disabled on this server");
                    return;
                }

                if ((string.IsNullOrEmpty(guildConfig.VerifiedMessage) || guildConfig.VerifiedMessage.Split().Length < 32 || !msg.GetGuild().Roles.Any(r => r.Id == guildConfig.VerifiedRole)))
                {
                    await msg.ReplyAsync(
                        $"It looks like verifiction is configured improperly (either the message is too short or the role does not exist.) Please contact your server administrator to resolve it.");
                    return;
                }

                List<SocketUser> failed = new List<SocketUser>();
                List<SocketUser> success = new List<SocketUser>();
                foreach (var user in users)
                {
                    string message = $"Hey {user.Username}! To get verified on **{msg.GetGuild().Name}** reply to this message with the hidden code in the message below\n\n"
                                     + GenericBot.GuildConfigs[msg.GetGuild().Id].VerifiedMessage;

                    string verificationMessage =
                        VerificationEngine.InsertCodeInMessage(message, VerificationEngine.GetVerificationCode(user.Id, msg.GetGuild().Id));

                    try
                    {
                        await user.GetOrCreateDMChannelAsync().Result.SendMessageAsync(verificationMessage);
                        success.Add(user);
                    }
                    catch (Exception ex)
                    {
                        failed.Add(user);
                    }
                }

                string reply = $"I've sent {success.Select(u => u.Username).ToList().SumAnd()} instructions!";
                if (failed.Any())
                {
                    reply += $" {failed.Select(u => u.Username).ToList().SumAnd()} could not be messaged.";
                }
                await msg.ReplyAsync(reply);
            };
            TestCommands.Add(verify);

            Command cmdp = new Command("cmd");
            cmdp.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            cmdp.ToExecute += async (client, msg, parameters) =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process cmd = new Process();
                    cmd.StartInfo.FileName = "cmd.exe";
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.RedirectStandardOutput = true;
                    cmd.StartInfo.CreateNoWindow = true;
                    cmd.StartInfo.UseShellExecute = false;
                    cmd.Start();

                    cmd.StandardInput.WriteLine(parameters.reJoin());
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                    cmd.WaitForExit();
                    foreach (var str in cmd.StandardOutput.ReadToEnd().SplitSafe('\n'))
                    {
                        await msg.ReplyAsync($"```\n{str}\n```");
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = "/bin/bash";
                    proc.StartInfo.Arguments = "-c \"" + parameters.reJoin() + " > results\"";
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;

                    proc.Start();
                    proc.WaitForExit();

                    Console.WriteLine(proc.StandardOutput.ReadToEnd());
                    foreach (string str in File.ReadAllText("results").SplitSafe('\n'))
                    {
                        await msg.ReplyAsync($"```\n{str}\n```");
                    }
                }
                else
                {
                    await msg.ReplyAsync("Unrecognized platform");
                }
            };
            TestCommands.Add(cmdp);

            Command decryptDb = new Command("decryptDb");
            decryptDb.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            decryptDb.ToExecute += async (client, msg, parameters) =>
            {
                File.WriteAllText($"files/guildDbs/{parameters[0]}_raw.json", AES.DecryptText(File.ReadAllText($"files/guildDbs/{parameters[0]}.json"), GenericBot.DBPassword));
                var res = msg.Channel.SendFileAsync($"files/guildDbs/{parameters[0]}_raw.json", "Self-destructing in 15 seconds!").Result;
                await Task.Delay(TimeSpan.FromSeconds(15));
                try { await res.DeleteAsync(); }
                catch { }
            };

            TestCommands.Add(decryptDb);

            Command repairDb = new Command("repairDb");
            repairDb.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            repairDb.ToExecute += async (client, msg, paramList) =>
            {
                lock (msg.GetGuild().Id.ToString())
                {
                    var db = new DBGuild(msg.GetGuild().Id);


                    foreach (var user in db.Users)
                    {
                        if (!user.Nicknames.Empty())
                        {
                            user.Nicknames = user.Nicknames.Where(n => !string.IsNullOrEmpty(n)).ToList();
                        }

                        if (!user.Usernames.Empty())
                        {
                            user.Usernames = user.Usernames.Where(n => !string.IsNullOrEmpty(n)).ToList();
                        }

                    }
                    db.Save();
                }
                await msg.ReplyAsync($"Done!");
            };

            TestCommands.Add(repairDb);

            return TestCommands;
        }
    }
}
