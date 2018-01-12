using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            config.Usage = "config <option> <value>`\nOptions are: `adminroles`, `moderatorroles`, `userroles`, `twitter`, `prefix";
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

                else await msg.ReplyAsync($"Unknown property `{paramList[0]}`.");

                GenericBot.GuildConfigs[msg.GetGuild().Id].Save();
            };

            ConfigCommands.Add(config);

            return ConfigCommands;
        }
    }
}
