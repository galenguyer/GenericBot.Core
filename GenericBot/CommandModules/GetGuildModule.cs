using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    class GetGuildModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();
            
            Command getGuildCommand = new Command("getguild");
            getGuildCommand.Usage = "getguild <user|guild|name>";
            getGuildCommand.SendTyping = true;
            getGuildCommand.Description = "Gets information about guilds this bot is in.";
            getGuildCommand.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            getGuildCommand.ToExecute = async (ctx) =>
            {
                if (MentionUtils.TryParseUser(ctx.ParameterString, out var userId))
                {
                    if (Core.DiscordClient.GetUser(userId) is SocketUser user)
                        await PrintUserGuilds(ctx, user);
                    else await ctx.Message.ReplyAsync($"User with ID `{userId}` not found.");
                }
                else if (ulong.TryParse(ctx.ParameterString, out var id))
                {
                    if (Core.DiscordClient.GetGuild(id) is SocketGuild guild)
                        await PrintGuildInformation(ctx, guild);
                    else if (Core.DiscordClient.GetUser(id) is SocketUser user)
                        await PrintUserGuilds(ctx, user);
                    else await ctx.Message.ReplyAsync($"User or guild with ID `{userId}` not found.");
                }
                else await PerformGuildSearch(ctx, ctx.ParameterString);
            };
            commands.Add(getGuildCommand);

            return commands;
        }

        private async Task PerformGuildSearch(ParsedCommand ctx, string searchQuery)
        {
            var foundGuilds = Core.DiscordClient.Guilds.Where(g => g.Name.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase)).ToList();

            if (foundGuilds.Count == 0)
            {
                await ctx.Message.ReplyAsync($"Search query `{searchQuery}` did not return any resulting guilds.");
                return;
            }
            
            if (foundGuilds.Count > 25)
            {
                await ctx.Message.ReplyAsync(
                    $"Search query `{searchQuery} resulted in **{foundGuilds.Count}** guilds. Try constraining your search term.");
                return;
            }
            
            var eb = new EmbedBuilder().WithTitle($"Guilds matching \"{searchQuery}\"");
            foreach (var foundGuild in foundGuilds)
                eb.AddField(foundGuild.Name, $"**ID:** {foundGuild.Id}\n**Members:** {foundGuild.MemberCount}\n**Owned by:** {foundGuild.Owner.Username}#{foundGuild.Owner.Discriminator} ({foundGuild.OwnerId})");

            await ctx.Channel.SendMessageAsync(embed: eb.Build());
        }

        private async Task PrintGuildInformation(ParsedCommand ctx, SocketGuild guild)
        {
            var eb = new EmbedBuilder()
                .WithTitle($"{guild.Name}")
                .AddField("ID", guild.Id)
                .AddField("Members", guild.MemberCount)
                .AddField("Created on", $"{guild.CreatedAt:yyyy-MM-dd HH\\:mm\\:ss zzzz} UTC")
                .AddField("Owner", $"{guild.Owner.Username}#{guild.Owner.Discriminator} ({guild.OwnerId})");
            await ctx.Channel.SendMessageAsync(embed: eb.Build());
        }

        private async Task PrintUserGuilds(ParsedCommand ctx, SocketUser user)
        {
            var eb = new EmbedBuilder().WithTitle($"Guilds owned by user {user.Username}#{user.Discriminator} ({user.Id})");
            var ownedGuilds = Core.DiscordClient.Guilds.Where(g => g.OwnerId == user.Id).Take(25).ToList();
            if (ownedGuilds.Count == 0)
            {
                await ctx.Message.ReplyAsync($"User {user.Username}#{user.Discriminator} ({user.Id}) does not own any guilds known by this bot.");
            }
            else
            {
                foreach (var ownedGuild in ownedGuilds)
                    eb.AddField(ownedGuild.Name, $"**ID:** {ownedGuild.Id}\n**Members:** {ownedGuild.MemberCount}");
                await ctx.Channel.SendMessageAsync(embed: eb.Build());
            }
        }
    }
}