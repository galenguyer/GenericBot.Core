using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using Discord;
using GenericBot.Entities;
using LiteDB;
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

                using (var db = new LiteDatabase(GenericBot.DBConnectionString))
                {
                    var col = db.GetCollection<DBGuild>("userDatabase");
                    col.EnsureIndex(c => c.ID, true);
                    DBGuild guildDb;
                    if(col.Exists(g => g.ID.Equals(msg.GetGuild().Id)))
                        guildDb = col.FindOne(g => g.ID.Equals(msg.GetGuild().Id));
                    else guildDb = new DBGuild (msg.GetGuild().Id);
                    foreach (var user in msg.GetGuild().Users)
                    {
                        if (!guildDb.Users.Any(u => u.ID.Equals(user.Id)))
                        {
                            guildDb.Users.Add(new DBUser(user));
                            newUsers++;
                        }
                    }
                    col.Upsert(guildDb);
                    db.Dispose();
                }
                await msg.ReplyAsync($"`{newUsers}` users added to database");
            };

            TestCommands.Add(updateDB);

            Command IdInfo = new Command("idInfo");
            IdInfo.Aliases = new List<string>{"id"};
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
                    long epochtime = (long) rawtime + 1420070400000;
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

                var guildDb  = new DBGuild().GetDBGuildFromId(msg.GetGuild().Id);

                info += $"Access time: `{stw.ElapsedMilliseconds}`ms\n";
                info += $"Registered Users: `{guildDb.Users.Count}`\n";

                int unc = 0, nnc = 0, wnc = 0, nuc = 0;
                foreach (var user in guildDb.Users)
                {
                    if(user.Usernames != null && user.Usernames.Any())
                        unc += user.Usernames.Count;
                    if(user.Nicknames != null && user.Nicknames.Any())
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


                await msg.ReplyAsync(info);
            };

            TestCommands.Add(DBStats);
                    {
                        if(user.Usernames != null && user.Usernames.Any())
                            unc += user.Usernames.Count;
                        if(user.Nicknames != null && user.Nicknames.Any())
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

                    db.Dispose();
                }
                await msg.ReplyAsync(info);
            };

            TestCommands.Add(DBStats);
            Command verify = new Command("verify");
            verify.RequiredPermission = Command.PermissionLevels.User;
            verify.ToExecute += async (client, msg, parameter) =>
            {
                List<SocketUser> users = new List<SocketUser>();

                if (parameter.Empty())
                {
                    users.Add(msg.Author);
                }
                else
                {
                    foreach (var user in msg.GetMentionedUsers())
                    {
                        users.Add(user);
                    }
                }

                var guildConfig = GenericBot.GuildConfigs[msg.GetGuild().Id];

                if (guildConfig.VerifiedRole == 0)
                {
                    await msg.ReplyAsync($"Verification is disabled on this server");
                    return;
                }

                if ((string.IsNullOrEmpty(guildConfig.VerifiedMessage) || guildConfig.VerifiedMessage.Split().Length < 64 || msg.GetGuild().Roles.Any(r => r.Id == guildConfig.VerifiedRole)))
                {
                    await msg.ReplyAsync(
                        $"It looks like verifiction is configured improperly (either the message is too short or the role does not exist.) Please contact your server administrator to resolve it.");
                    return;
                }

                string message = $"To get verified on **{msg.GetGuild().Name}** reply to this message with the hidden code in the message below\n\n"
                                 + GenericBot.GuildConfigs[msg.GetGuild().Id].VerifiedMessage;

                int wc = message.Length;

                int sPos = new Random().Next((wc/2), wc);
                for (int i = sPos; i < wc; i++)
                {
                    if (message[i].Equals(' '))
                        break;
                    sPos++;
                }

                message = message.Substring(0, sPos) + $" *(the secret is: {GetVerificationCode(msg.Author.Id, msg.GetGuild().Id)})* " + message.Substring(sPos);

                await msg.ReplyAsync(message);
            };
            TestCommands.Add(verify);

            Command cmdp = new Command("cmd");
            cmdp.RequiredPermission = Command.PermissionLevels.BotOwner;
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
                    Process proc = new System.Diagnostics.Process ();
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


            return TestCommands;
        }

        private string GetVerificationCode(ulong userId, ulong guildId)
        {
            var pid = int.Parse(userId.ToString().Substring(7, 6));
            var gid = int.Parse(guildId.ToString().Substring(7, 6));

            return (gid + pid).ToString("X").ToLower();
        }

        private SocketGuild GetGuildFromCode(string code, ulong userId)
        {
            var pid = int.Parse(userId.ToString().Substring(7, 6));
            var sum = Convert.ToInt32(code, 16);
            var gid = sum - pid;

            if (GenericBot.DiscordClient.Guilds.HasElement(g => g.Id.ToString().Contains(gid.ToString()),
                out SocketGuild guild))
                return guild;
            return null;
        }
    }
}
