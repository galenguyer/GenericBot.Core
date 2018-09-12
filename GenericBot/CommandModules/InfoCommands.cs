using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericBot.CommandModules
{
    class InfoCommands
    {
        public List<Command> GetInfoCommands()
        {
            List<Command> infoCommands = new List<Command>();

            Command info = new Command("info");
            info.Description = "Send an informational card about the bot";
            info.ToExecute += async (client, msg, parameters) =>
            {
                string prefix = GenericBot.GlobalConfiguration.DefaultPrefix;
                if (!String.IsNullOrEmpty(GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix))
                    prefix = GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix;

                string config = info.GetPermissions(msg.Author, msg.GetGuild().Id) >= Command.PermissionLevels.Admin ? $" Admins can also run `{prefix}confighelp` to see everything you can set up" : "";

                var builder = new EmbedBuilder()
                    .WithTitle("GenericBot: An All-Purpose Almost-Decent Bot")
                    .WithDescription("GenericBot aims to provide an almost full featured moderation and fun box experience in one convenient package")
                    .WithUrl("https://github.com/MasterChief-John-117/GenericBot")
                    .WithColor(new Color(0xFF))
                    .WithFooter(footer =>
                    {
                        footer
                            .WithText($"Made by {GenericBot.DiscordClient.GetUser(169918990313848832).ToString()} | Hosted by {GenericBot.DiscordClient.GetUser(152905790959779840).ToString()}")
                            .WithIconUrl(GenericBot.DiscordClient.GetUser(169918990313848832).GetAvatarUrl());
                    })
                    .WithThumbnailUrl(GenericBot.DiscordClient.CurrentUser.GetAvatarUrl().Replace("size=128", "size=2048"))
                    .AddField($"Links", $"GenericBot is currently in a closed state, however if you wish to use it in your own server please get in contact with the developer, whose username is in the footer\nAlso, the source code is public on [github](https://github.com/MasterChief-John-117/GenericBot). You can also open bug reports on GitHub ")
                    .AddField($"Getting Started", $"See everything you can make me do with `{prefix}help`. {config}")
                    .AddField($"Self Assignable Roles", $"One of the most common public features GenericBot is used for is roles a user can assign to themself. To see all the avaible roles, do `{prefix}userroles`. You can join a role with `{prefix}iam [rolename]` or leave a role with `{prefix}iamnot [rolename]`.")
                    .AddField($"Moderation", $"GenericBot provides a wide range of tools for moderators to track users and infractions. It keeps track of all of a user's usernames, nicknames, and logged infractions, including kicks and timed or permanent bans. Users can be searched for either by ID, or by username or nickname, whether it be current or an old name. (All data is stored in an encrypted database, and data from one server is completely inaccessible by another server)")
                    .AddField($"Fun!", $"In addition to being a highly effective moderator toolkit, GenericBot has some fun commands, such as `{prefix}dog`, `{prefix}cat`, or `{prefix}jeff`. You can also create your own custom commands for rapid-fire memery or whatever else tickles your fancy");
                var embed = builder.Build();

                await msg.Channel.SendMessageAsync("", embed: embed);
            };

            infoCommands.Add(info);

            Command configinfo = new Command("configinfo");
            configinfo.RequiredPermission = Command.PermissionLevels.Admin;
            configinfo.Description = "Show all the options to configure with syntax for each";
            configinfo.ToExecute += async (client, msg, parameters) =>
            {
                string prefix = GenericBot.GlobalConfiguration.DefaultPrefix;
                if (!String.IsNullOrEmpty(GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix))
                    prefix = GenericBot.GuildConfigs[(msg.Channel as SocketGuildChannel).Guild.Id].Prefix;

                var builder = new EmbedBuilder()
                    .WithTitle("GenericBot: Config Information")
                    .WithDescription($"The `{prefix}config` command is huge and confusing. This aims to make it a bit simpler (For more general assistance, try `{prefix}info`)")
                    .WithUrl("https://github.com/MasterChief-John-117/GenericBot")
                    .WithColor(new Color(0xEF4347))
                    .WithFooter(footer =>
                    {
                        footer
                            .WithText($"If you have questions or notice any errors, please contact {GenericBot.DiscordClient.GetUser(169918990313848832).ToString()}");
                    })
                    .WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/1/10/Gear_1.svg/1000px-Gear_1.svg.png")
                    .AddField("AdminRoles", $"Add or remove Admin Roles by ID\nSyntax: `{prefix}config adminroles <add/remove> [roleId]`")
                    .AddField("ModeratorRoles (ModRoles)", $"Add or remove Moderator Roles by ID\nSyntax: `{prefix}config modroles <add/remove> [roleId]`")
                    .AddField("UserRoles", $"Add or remove User-Assignable Roles by ID\nSyntax: `{prefix}config userroles <add/remove> [roleId]`")
                    .AddField("Twitter", $"Enable or Disable tweeting from the server through the bot\nSyntax: `{prefix}config twitter <true/false>`")
                    .AddField("Prefix", $"Set the prefix to a given string. If [prefixString] is empty it gets set to the default of `{GenericBot.GlobalConfiguration.DefaultPrefix}`\nSyntax: `{prefix}config prefix [prefixString]`")
                    .AddField("Logging", $"Set the channel for logging by Id\nSyntax: `{prefix}config logging channelId [channelId]`\n\nToggle ignoring channels for logging by Id. Lists all ignored channels if channelId is empty\nSyntax`{prefix}config logging ignoreChannel [channelId]`")
                    .AddField("MutedRoleId", $"Set the role assigned by the `{prefix}mute` command. Set [roleId] to `0` to disable muting\nSyntax: `{prefix}config mutedRoleId [roleId]`")
                    .AddField("Verification", $"Get or Set the RoleId assigned for verification. Leave [roleId] empty to get the current role. Use `0` for the [roleId] to disable verification\nSyntax: `{prefix}config verification roleId [roleId]`\n\nGet or set the message sent for verification. Leave [message] empty to get the current message\nSyntax: `{prefix}config verification message [message]`")
                    .AddField("Points", $"Toggle whether points are enabled on the server\nSyntax: `{prefix}config points enabled`")
                    .AddField("GlobalBanOptOut", $"If a user has been proved to be engaging in illegal acts such as distributing underage porn, sometimes the bot owner will ban them from all servers the bot is in. You can opt out of this if you want\nSyntax: `{prefix}config globalbanoptout <true/false>`")
                    .AddField("AutoRole", $"Add or remove a role to be automatically granted by Id\nSyntax: `{prefix}config autorole <add/remove> [roleId]`");
                var embed = builder.Build();

                await msg.Channel.SendMessageAsync("", embed: embed);
            };

            infoCommands.Add(configinfo);

            return infoCommands;
        }
    }
}
