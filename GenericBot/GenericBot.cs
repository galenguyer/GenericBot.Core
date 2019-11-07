using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GenericBot.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace GenericBot
{
    class GenericBot
    {
        public static string BuildId = string.Empty;
        public static List<ulong> ClearedMessageIds = new List<ulong>();
        static void Main(string[] args)
        {
            if (File.Exists("version.txt"))
            {
                Core.Logger.LogGenericMessage($"Build {File.ReadAllText("version.txt").Trim()}");
                BuildId = File.ReadAllText("version.txt").Trim();
            }

            Timer cycleTimer = new Timer();
            cycleTimer.Interval = 60 * 1000;
            cycleTimer.AutoReset = true;
            cycleTimer.Elapsed += ExecuteCycle;
            cycleTimer.Start();

            Start().GetAwaiter().GetResult();
        }

        private static void ExecuteCycle(object sender, ElapsedEventArgs e)
        {
            var status = new Status();
            Core.AddStatus(status);

            // Check for unbans
            foreach(var gid in Core.DiscordClient.Guilds.Select(g => g.Id))
            {
                var bans = Core.GetBansFromGuild(gid, false);
                foreach(var ban in bans.Where(b => b.BannedUntil < DateTime.UtcNow))
                {
                    try
                    {
                        var user = Core.DiscordClient.GetGuild(ban.GuildId).GetBansAsync().Result
                        .First(b => b.User.Id == ban.Id).User;
                        Core.DiscordClient.GetGuild(gid).RemoveBanAsync(ban.Id);

                        var builder = new EmbedBuilder()
                            .WithTitle("User Unbanned")
                            .WithDescription($"Banned for: {ban.Reason}")
                            .WithColor(new Color(0xFFFF00))
                            .WithFooter(footer => {
                                footer
                                    .WithText($"{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")} GMT");
                            })
                            .WithAuthor(author => {
                                author
                                    .WithName(user.ToString())
                                    .WithIconUrl(user.GetAvatarUrl());
                            })
                            .AddField(new EmbedFieldBuilder().WithName("All Warnings").WithValue(
                                Core.GetUserFromGuild(ban.Id, gid).Warnings.SumAnd()));
                        ((SocketTextChannel)Core.DiscordClient.GetChannel(Core.GetGuildConfig(gid).LoggingChannelId))
                            .SendMessageAsync("", embed: builder.Build());
                    }
                    catch(Exception ex)
                    {
                        Core.Logger.LogErrorMessage(ex, null);
                    }
                    try
                    {
                        Core.RemoveBanFromGuild(ban.Id, gid);
                    }
                    catch (Exception ex)
                    {
                        Core.Logger.LogErrorMessage(ex, null);
                    }
                }
            }
        }

        private static async Task Start()
        {
            try
            {
                await Core.DiscordClient.LoginAsync(TokenType.Bot, Core.GlobalConfig.DiscordToken);
                await Core.DiscordClient.StartAsync();
            }
            catch (Exception e)
            {
                await Core.Logger.LogErrorMessage(e, null);
                return;
            }

            // Block until exited
            await Task.Delay(-1);
        }
    }
}
