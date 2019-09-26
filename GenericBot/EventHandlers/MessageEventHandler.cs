using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;
using Newtonsoft.Json;

namespace GenericBot
{
    public static class MessageEventHandler
    {
        public static async Task MessageRecieved(SocketMessage parameterMessage, bool edited = false)
        {
            // Don't do stuff if the user is blacklisted
            if (Core.CheckBlacklisted(parameterMessage.Author.Id))
                return;
            // Ignore self
            if (parameterMessage.Author.Id == Core.GetCurrentUserId())
                return;
            try
            {
                ulong guildId = parameterMessage.GetGuild().Id;
                var command = new Command("t").ParseMessage(parameterMessage);

                if (Core.GetCustomCommands(guildId).Result.HasElement(c => c.Name == command.Name, 
                    out CustomCommand customCommand))
                {
                    if (customCommand.Delete)
                        await parameterMessage.DeleteAsync();
                    await parameterMessage.ReplyAsync(customCommand.Response);
                }

                if(command != null && command.RawCommand != null)
                    await command.Execute();
            }
            catch (Exception ex)
            {
                if (parameterMessage.Author.Id == Core.GetOwnerId())
                {
                    await parameterMessage.ReplyAsync("```\n" + $"{ex.Message}\n{ex.StackTrace}".SafeSubstring(1000) +
                                                      "\n```");
                }
                await Core.Logger.LogErrorMessage(ex.Message);
                Console.WriteLine($"{ex.StackTrace}");
            }
        }

        public static async Task MessageRecieved(SocketMessage arg)
        {
            await MessageRecieved(arg, edited: false);
        }

        public static async Task HandleEditedCommand(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            if (arg1.Value.Content == arg2.Content) return;

            if (Core.GlobalConfig.DefaultExecuteEdits)
            {
                await MessageEventHandler.MessageRecieved(arg2, edited: true);
            }

            var guildConfig = Core.GetGuildConfig(arg2.GetGuild().Id);

            if (guildConfig.LoggingChannelId == 0 || guildConfig.MessageLoggingIgnoreChannels.Contains(arg2.Channel.Id)
                                                  || !arg1.HasValue) return;

            EmbedBuilder log = new EmbedBuilder()
                .WithTitle("Message Edited")
                .WithColor(243, 110, 33)
                .WithCurrentTimestamp();

            if (string.IsNullOrEmpty(arg2.Author.GetAvatarUrl()))
            {
                log = log.WithAuthor(new EmbedAuthorBuilder().WithName($"{arg2.Author} ({arg2.Author.Id})"));
            }
            else
            {
                log = log.WithAuthor(new EmbedAuthorBuilder().WithName($"{arg2.Author} ({arg2.Author.Id})")
                    .WithIconUrl(arg2.Author.GetAvatarUrl() + " "));
            }

            log.AddField(new EmbedFieldBuilder().WithName("Channel").WithValue("#" + arg2.Channel.Name).WithIsInline(true));
            log.AddField(new EmbedFieldBuilder().WithName("Sent At").WithValue(arg1.Value.Timestamp.ToString(@"yyyy-MM-dd HH:mm.ss") + "GMT").WithIsInline(true));

            log.AddField(new EmbedFieldBuilder().WithName("Before").WithValue(arg1.Value.Content.SafeSubstring(1016)));
            log.AddField(new EmbedFieldBuilder().WithName("After").WithValue(arg2.Content.SafeSubstring(1016)));

            await arg2.GetGuild().GetTextChannel(guildConfig.LoggingChannelId).SendMessageAsync("", embed: log.Build());
        }

        public static async Task MessageDeleted(Cacheable<IMessage, ulong> arg, ISocketMessageChannel channel)
        {
            if (!arg.HasValue) return;
            if (GenericBot.ClearedMessageIds.Contains(arg.Id)) return;
            var guildConfig = Core.GetGuildConfig((arg.Value as SocketMessage).GetGuild().Id);

            if (guildConfig.LoggingChannelId == 0 || guildConfig.MessageLoggingIgnoreChannels.Contains(channel.Id)) return;

            EmbedBuilder log = new EmbedBuilder()
                .WithTitle("Message Deleted")
                .WithColor(139, 0, 0)
                .WithCurrentTimestamp();

            if (string.IsNullOrEmpty(arg.Value.Author.GetAvatarUrl()))
            {
                log = log.WithAuthor(new EmbedAuthorBuilder().WithName($"{arg.Value.Author} ({arg.Value.Author.Id})"));
            }
            else
            {
                log = log.WithAuthor(new EmbedAuthorBuilder().WithName($"{arg.Value.Author} ({arg.Value.Author.Id})")
                    .WithIconUrl(arg.Value.Author.GetAvatarUrl() + " "));
            }

            log.AddField(new EmbedFieldBuilder().WithName("Channel").WithValue("#" + arg.Value.Channel.Name).WithIsInline(true));
            log.AddField(new EmbedFieldBuilder().WithName("Sent At").WithValue(arg.Value.Timestamp.ToString(@"yyyy-MM-dd HH:mm.ss") + "GMT").WithIsInline(true));


            if (!string.IsNullOrEmpty(arg.Value.Content))
            {
                log.WithDescription("**Message:** " + arg.Value.Content);
            }

            if (arg.Value.Attachments.Any())
            {
                log.AddField(new EmbedFieldBuilder().WithName("Attachments").WithValue(arg.Value.Attachments.Select(a =>
                    $"File: {a.Filename}").Aggregate((a, b) => a + "\n" + b)));
                log.WithImageUrl(arg.Value.Attachments.First().ProxyUrl);
            }

            if (string.IsNullOrEmpty(arg.Value.Content) && !arg.Value.Attachments.Any() && arg.Value.Embeds.Any())
            {
                log.WithDescription("**Embed:**\n```json\n" + JsonConvert.SerializeObject(arg.Value.Embeds.First(), Formatting.Indented) + "\n```");
            }

            log.Footer = new EmbedFooterBuilder().WithText(arg.Value.Id.ToString());

            await (arg.Value as SocketMessage).GetGuild().GetTextChannel(guildConfig.LoggingChannelId).SendMessageAsync("", embed: log.Build());
        }
    }
}