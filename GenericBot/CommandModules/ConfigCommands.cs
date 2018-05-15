using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using GenericBot.Entities;
using Newtonsoft.Json;

namespace GenericBot.CommandModules
{
    public class ConfigCommands
    {
        public List<Command> GetConfigComamnds()
        {
            List<Command> ConfigCommands = new List<Command>();

            Command config = new Command("config");
            config.Usage = "config <option> <value>`\nOptions are: `adminroles`, `moderatorroles`, `userroles`, `twitter`, `voicerole, `user`, `mutedroleid";
            config.Description = "Configure the bot's option";
            config.RequiredPermission = Command.PermissionLevels.Admin;
            config.ToExecute += async (client, msg, paramList) =>
            {
                if (paramList.Empty())
                {
                    await msg.Channel.SendMessageAsync($"Please enter a value to configure.");
                    return;
                }
                #region AdminRoles
                if (paramList[0].ToLower().Equals("adminroles"))
                {
                    if (config.GetPermissions(msg.Author, msg.GetGuild().Id) >= Command.PermissionLevels.GuildOwner)
                    {
                        if (paramList.Count == 1)
                        {
                            await msg.ReplyAsync($"Please enter a config option");
                            return;
                        }
                        if (paramList.Count == 2)
                        {
                            await msg.ReplyAsync($"Please enter a roleId");
                            return;
                        }
                        ulong id;
                        if (ulong.TryParse(paramList[2], out id) && msg.GetGuild().Roles.Select(r => r.Id).Any(u => u.Equals(id)))
                        {
                            if (paramList[1].ToLower().Equals("add"))
                            {
                                if (!GenericBot.GuildConfigs[msg.GetGuild().Id].AdminRoleIds.Contains(id))
                                {
                                    GenericBot.GuildConfigs[msg.GetGuild().Id].AdminRoleIds.Add(id);
                                    await msg.ReplyAsync($"Added {msg.GetGuild().Roles.FirstOrDefault(r => r.Id == id).Name} to Admin Roles");
                                }
                                else{await msg.ReplyAsync($"Admin Roles already contains {msg.GetGuild().Roles.FirstOrDefault(r => r.Id == id).Name}");}
                            }
                            else if (paramList[1].ToLower().Equals("remove"))
                            {
                                if (GenericBot.GuildConfigs[msg.GetGuild().Id].AdminRoleIds.Contains(id))
                                {
                                    GenericBot.GuildConfigs[msg.GetGuild().Id].AdminRoleIds.Remove(id);
                                    await msg.ReplyAsync($"Removed {msg.GetGuild().Roles.FirstOrDefault(r => r.Id == id).Name} from Admin Roles");
                                }
                                else
                                {
                                    await msg.ReplyAsync(
                                        $"Admin Roles doesn't contain {msg.GetGuild().Roles.FirstOrDefault(r => r.Id == id).Name}");
                                }
                            }
                            else{await msg.ReplyAsync($"Unknown property `{paramList[1]}`.");}
                        }
                        else{await msg.ReplyAsync($"That is not a valid roleId");}
                    }
                    else{await msg.ReplyAsync($"You don't have the permissions to do that");}
                }
                #endregion AdminRoles

                #region ModRoles

                else if (paramList[0].ToLower().Equals("moderatorroles") || paramList[0].ToLower().Equals("modroles"))
                {
                    if (config.GetPermissions(msg.Author, msg.GetGuild().Id) >= Command.PermissionLevels.Admin)
                    {
                        if (paramList.Count == 1)
                        {
                            await msg.ReplyAsync($"Please enter a config option");
                            return;
                        }
                        if (paramList.Count == 2)
                        {
                            await msg.ReplyAsync($"Please enter a roleId");
                            return;
                        }
                        ulong id;
                        if (ulong.TryParse(paramList[2], out id) && msg.GetGuild().Roles.Select(r => r.Id).Any(u => u.Equals(id)))
                        {
                            if (paramList[1].ToLower().Equals("add"))
                            {
                                if (!GenericBot.GuildConfigs[msg.GetGuild().Id].ModRoleIds.Contains(id))
                                {
                                    GenericBot.GuildConfigs[msg.GetGuild().Id].ModRoleIds.Add(id);
                                    await msg.ReplyAsync($"Added {msg.GetGuild().Roles.FirstOrDefault(r => r.Id == id).Name} to Moderator Roles");
                                }
                                else
                                {
                                    await msg.ReplyAsync($"Moderator Roles already contains {msg.GetGuild().Roles.FirstOrDefault(r => r.Id == id).Name}");
                                }
                            }
                            else if (paramList[1].ToLower().Equals("remove"))
                            {
                                if (GenericBot.GuildConfigs[msg.GetGuild().Id].ModRoleIds.Contains(id))
                                {
                                    {
                                        GenericBot.GuildConfigs[msg.GetGuild().Id].ModRoleIds.Remove(id);
                                        await msg.ReplyAsync(
                                            $"Removed {msg.GetGuild().Roles.FirstOrDefault(r => r.Id == id).Name} from Moderator Roles");
                                    }
                                }
                                else
                                {
                                    await msg.ReplyAsync(
                                        $"Moderator Roles doesn't contain {msg.GetGuild().Roles.FirstOrDefault(r => r.Id == id).Name}");
                                }
                            }
                            else{await msg.ReplyAsync($"Unknown property `{paramList[1]}`.");}
                        }
                        else{await msg.ReplyAsync($"That is not a valid roleId");}
                    }
                    else{await msg.ReplyAsync($"You don't have the permissions to do that");}
                }

                #endregion ModRoles

                #region UserRoles

                else if (paramList[0].ToLower().Equals("userroles"))
                {
                    if (config.GetPermissions(msg.Author, msg.GetGuild().Id) >= Command.PermissionLevels.Admin)
                    {
                        if (paramList.Count == 1)
                        {
                            await msg.ReplyAsync($"Please enter a config option");
                            return;
                        }
                        if (paramList.Count == 2)
                        {
                            await msg.ReplyAsync($"Please enter a roleId");
                            return;
                        }
                        ulong id;
                        if (ulong.TryParse(paramList[2], out id) && msg.GetGuild().Roles.Select(r => r.Id).Any(u => u.Equals(id)))
                        {
                            if (paramList[1].ToLower().Equals("add"))
                            {
                                if (!GenericBot.GuildConfigs[msg.GetGuild().Id].UserRoleIds.Contains(id))
                                {
                                    GenericBot.GuildConfigs[msg.GetGuild().Id].UserRoleIds.Add(id);
                                    await msg.ReplyAsync($"Added {msg.GetGuild().Roles.FirstOrDefault(r => r.Id == id).Name} to User Roles");
                                }
                                else
                                {
                                    await msg.ReplyAsync($"User Roles already contains {msg.GetGuild().Roles.FirstOrDefault(r => r.Id == id).Name}");
                                }
                            }
                            else if (paramList[1].ToLower().Equals("remove"))
                            {
                                if (GenericBot.GuildConfigs[msg.GetGuild().Id].UserRoleIds.Contains(id))
                                {
                                    {
                                        GenericBot.GuildConfigs[msg.GetGuild().Id].UserRoleIds.Remove(id);
                                        await msg.ReplyAsync(
                                            $"Removed {msg.GetGuild().Roles.FirstOrDefault(r => r.Id == id).Name} from User Roles");
                                    }
                                }
                                else{await msg.ReplyAsync($"User Roles doesn't contain {msg.GetGuild().Roles.FirstOrDefault(r => r.Id == id).Name}");}
                            }
                            else{await msg.ReplyAsync($"Unknown property `{paramList[1]}`.");}
                        }
                        else{await msg.ReplyAsync($"That is not a valid roleId");}
                    }
                    else{await msg.ReplyAsync($"You don't have the permissions to do that");}
                }

                #endregion UserRoles

                #region Twitter

                else if (paramList[0].ToLower().Equals("twitter"))
                {
                    if (paramList[1].ToLower() == "true")
                    {
                        GenericBot.GuildConfigs[msg.GetGuild().Id].AllowTwitter = true;
                        await msg.ReplyAsync("Tweeting on this server has been enabled");
                    }
                    else if (paramList[1].ToLower() == "false")
                    {
                        GenericBot.GuildConfigs[msg.GetGuild().Id].AllowTwitter = false;
                        await msg.ReplyAsync("Tweeting on this server has been disabled");
                    }
                    else
                    {
                        await msg.ReplyAsync("That's not a valid option");
                    }
                }

                #endregion Twitter

                #region Prefix

                else if (paramList[0].ToLower().Equals("prefix"))
                {
                    try
                    {
                        paramList.RemoveAt(0);
                        GenericBot.GuildConfigs[msg.GetGuild().Id].Prefix = paramList.reJoin();
                        if (msg.Content.EndsWith('"') && paramList[0].ToCharArray()[0].Equals('"'))
                        {
                            GenericBot.GuildConfigs[msg.GetGuild().Id].Prefix = new Regex("\"(.*?)\"").Match(msg.Content).Value.Trim('"');
                        }
                        await msg.ReplyAsync($"The prefix has been set to `{GenericBot.GuildConfigs[msg.GetGuild().Id].Prefix}`");
                    }
                    catch (Exception Ex)
                    {
                        GenericBot.GuildConfigs[msg.GetGuild().Id].Prefix = "";
                        await msg.ReplyAsync($"The prefix has been reset to the default of `{GenericBot.GlobalConfiguration.DefaultPrefix}`");
                    }
                }

                #endregion Prefix

                #region UserEvents

                else if (paramList[0].ToLower().Equals("logging"))
                {
                    if (paramList[1].ToLower().Equals("channelid"))
                    {
                        if (paramList.Count() == 2)
                        {
                            await msg.ReplyAsync(
                                $"Current user event channel: <#{GenericBot.GuildConfigs[msg.GetGuild().Id].UserLogChannelId}>");
                        }
                        else
                        {
                            ulong cId;
                            if (ulong.TryParse(paramList[2], out cId) && (msg.GetGuild().Channels.Any(c => c.Id == cId) || cId == 0))
                            {
                                GenericBot.GuildConfigs[msg.GetGuild().Id].UserLogChannelId = cId;
                                await msg.ReplyAsync(
                                    $"User event channel set to <#{GenericBot.GuildConfigs[msg.GetGuild().Id].UserLogChannelId}>");
                            }
                            else await msg.ReplyAsync("Invalid Channel Id");
                        }
                    }
                    else if (paramList[1].ToLower().Equals("joinmessage"))
                    {
                        if (paramList.Count() == 2)
                        {
                            await msg.ReplyAsync(
                                $"Current user joined message: `{GenericBot.GuildConfigs[msg.GetGuild().Id].UserJoinedMessage.Replace("`", "'")}`");
                        }
                        else
                        {
                            paramList.RemoveRange(0, 2);
                            GenericBot.GuildConfigs[msg.GetGuild().Id].UserJoinedMessage = paramList.reJoin();
                            await msg.ReplyAsync($"User Join message set to `{GenericBot.GuildConfigs[msg.GetGuild().Id].UserJoinedMessage.Replace("`", "'")}`");
                        }
                    }
                    else if (paramList[1].ToLower().Equals("leavemessage"))
                    {
                        if (paramList.Count() == 2)
                        {
                            await msg.ReplyAsync(
                                $"Current user left message: `{GenericBot.GuildConfigs[msg.GetGuild().Id].UserLeftMessage.Replace("`", "'")}`");
                        }
                        else
                        {
                            paramList.RemoveRange(0, 2);
                            GenericBot.GuildConfigs[msg.GetGuild().Id].UserLeftMessage = paramList.reJoin();
                            await msg.ReplyAsync($"User Join message set to `{GenericBot.GuildConfigs[msg.GetGuild().Id].UserLeftMessage.Replace("`", "'")}`");
                        }
                    }
                    else if (paramList[1].ToLower().Equals("info"))
                    {
                        if (paramList[2].ToLower().Equals(("true")))
                        {
                            GenericBot.GuildConfigs[msg.GetGuild().Id].UserJoinedShowModNotes = true;
                            GenericBot.GuildConfigs[msg.GetGuild().Id].UserLogTimestamp= true;
                        }
                        else if (paramList[2].ToLower().Equals(("false")))
                        {
                            GenericBot.GuildConfigs[msg.GetGuild().Id].UserJoinedShowModNotes = false;
                            GenericBot.GuildConfigs[msg.GetGuild().Id].UserLogTimestamp= false;
                        }
                        else
                        {
                            await msg.ReplyAsync("Invalid Option");
                        }
                    }
                    else if (paramList[1].ToLower().Equals("ignorechannel"))
                    {
                        if (paramList.Count == 2)
                        {
                            string m = "Ignored channels:";
                            foreach (var id in GenericBot.GuildConfigs[msg.GetGuild().Id].MessageLoggingIgnoreChannels)
                            {
                                m += $"\n<#{id}>";
                            }

                            await msg.ReplyAsync(m);
                        }
                        else
                        {
                            if (ulong.TryParse(paramList[2], out ulong cId) &&
                                msg.GetGuild().TextChannels.Any(c => c.Id == cId))
                            {
                                if (!GenericBot.GuildConfigs[msg.GetGuild().Id].MessageLoggingIgnoreChannels.Contains(cId))
                                {
                                    GenericBot.GuildConfigs[msg.GetGuild().Id].MessageLoggingIgnoreChannels.Add(cId);
                                    await msg.ReplyAsync($"No longer logging <#{cId}>");
                                }
                                else
                                {
                                    GenericBot.GuildConfigs[msg.GetGuild().Id].MessageLoggingIgnoreChannels.Remove(cId);
                                    await msg.ReplyAsync($"Resuming logging <#{cId}>");
                                }
                            }
                            else
                            {
                                await msg.ReplyAsync("Invalid Channel Id");
                            }
                        }
                    }
                }

                #endregion UserEvents

                #region VoiceChannelRoles

                else if (paramList[0].ToLower().Equals("voicerole"))
                {
                    paramList.RemoveAt(0);
                    if (paramList.Count != 2)
                    {
                        await msg.ReplyAsync("Incorrect number of arguments. Make sure the command is `voicerole [VoiceChannelId] [RoleId]`");
                        return;
                    }
                    ulong channelId, roleId;
                    if (ulong.TryParse(paramList[0], out channelId) && ulong.TryParse(paramList[1], out roleId))
                    {
                        SocketVoiceChannel vc;
                        SocketRole role;
                        if (msg.GetGuild().VoiceChannels.HasElement(c => c.Id == channelId, out vc) &&
                            msg.GetGuild().Roles.HasElement(r => r.Id == roleId, out role))
                        {
                            GenericBot.GuildConfigs[msg.GetGuild().Id].VoiceChannelRoles.Override(channelId, roleId);
                            GenericBot.GuildConfigs[msg.GetGuild().Id].Save();
                            await msg.ReplyAsync($"Anyone who joins the {vc.Name} voice channel will get the {role.Name} role!");
                        }
                        else
                        {
                            await msg.ReplyAsync("One of those IDs does not exist on this server");
                        }
                    }
                    else
                    {
                        await msg.ReplyAsync(("Role or Channel ID is in an incorrect format"));
                    }

                }

                #endregion VoiceChannelRoles

                #region MutedRoleId

                else if (paramList[0].ToLower().Equals("mutedroleid"))
                {
                    paramList.RemoveAt(0);
                    if (paramList.Count != 1)
                    {
                        await msg.ReplyAsync(
                            "Incorrect number of arguments. Make sure the command is `voicerole [VoiceChannelId] [RoleId]`");
                        return;
                    }
                    else
                    {
                        ulong roleId;
                        if (ulong.TryParse(paramList[0], out roleId) && (msg.GetGuild().Roles.Any(r => r.Id == roleId) || roleId == 0))
                        {
                            GenericBot.GuildConfigs[msg.GetGuild().Id].MutedRoleId = roleId;
                            GenericBot.GuildConfigs[msg.GetGuild().Id].Save();
                            await msg.ReplyAsync($"MutedRoleId is now `{roleId}`");
                        }
                        else
                        {
                            await msg.ReplyAsync("Invalid Role Id");
                        }
                    }
                }

                #endregion MutedRoleId

                #region Verification

                else if (paramList[0].ToLower().Equals("verification"))
                {
                    if (paramList[1].ToLower().Equals("roleid"))
                    {
                        if (paramList.Count == 2)
                        {
                            var roleId = GenericBot.GuildConfigs[msg.GetGuild().Id].VerifiedRole;
                            if (roleId == 0)
                            {
                                await msg.ReplyAsync("Verification is disabled on this server");
                            }
                            else
                            {
                                await msg.ReplyAsync(
                                    $"Verification role is  `{msg.GetGuild().Roles.First(g => g.Id == roleId).Name}`");
                            }
                        }
                        else if(ulong.TryParse(paramList[2], out ulong roleId) && (msg.GetGuild().Roles.Any(g => g.Id == roleId) || roleId == 0))
                        {
                            GenericBot.GuildConfigs[msg.GetGuild().Id].VerifiedRole = roleId;
                            if (roleId != 0)
                            {
                                await msg.ReplyAsync(
                                    $"Verification role set to `{msg.GetGuild().Roles.First(g => g.Id == roleId).Name}`");
                            }
                            else
                            {
                                await msg.ReplyAsync("Verification role cleared. Verification is off for this server.");
                            }
                        }
                        else
                        {
                            await msg.ReplyAsync("Invalid RoleId");
                        }
                    }
                    else if (paramList[1].ToLower().Equals("message"))
                    {
                        string pref = GenericBot.GlobalConfiguration.DefaultPrefix;
                        if (!String.IsNullOrEmpty(GenericBot.GuildConfigs[msg.GetGuild().Id].Prefix))
                            pref = GenericBot.GuildConfigs[msg.GetGuild().Id].Prefix;

                        string message = msg.Content;
                        message = message.Remove(0, pref.Length).TrimStart(' ').Remove(0, "config".Length).TrimStart(' ').Remove(0, "verification".Length).TrimStart(' ').Remove(0, "message".Length).Trim(' ');

                        GenericBot.GuildConfigs[msg.GetGuild().Id].VerifiedMessage = message;

                        await msg.ReplyAsync("Example verification message:");

                        string vm = $"Hey {msg.Author.Username}! To get verified on **{msg.GetGuild().Name}** reply to this message with the hidden code in the message below\n\n"
                                         + GenericBot.GuildConfigs[msg.GetGuild().Id].VerifiedMessage;

                        string verificationMessage =
                            VerificationEngine.InsertCodeInMessage(vm, VerificationEngine.GetVerificationCode(msg.Author.Id, msg.GetGuild().Id));

                        await msg.ReplyAsync(verificationMessage);
                    }
                    else await msg.ReplyAsync("Invalid Option");
                }

                #endregion Verification

                else await msg.ReplyAsync($"Unknown property `{paramList[0]}`.");

                GenericBot.GuildConfigs[msg.GetGuild().Id].Save();
            };

            ConfigCommands.Add(config);

            return ConfigCommands;
        }
    }
}
