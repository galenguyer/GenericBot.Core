using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot
{
    public static class MessageEventHandler
    {
        public static async Task MessageRecieved(SocketMessage parameterMessage)
        {
            // Don't handle the command if it is a system message
            var message = parameterMessage;

            GenericBot.MessageCounter++;
            GenericBot.Latency = (int) Math.Round((DateTimeOffset.UtcNow - parameterMessage.Timestamp).TotalMilliseconds);


            if (GenericBot.GlobalConfiguration.BlacklistedIds.Contains(message.Author.Id))
            {
                return;
            }

            if(parameterMessage.Author.Id == GenericBot.DiscordClient.CurrentUser.Id)
                return;

            if (parameterMessage.Channel.GetType().FullName.ToLower().Contains("dmchannel"))
            {
                if (message.Author.Id == GenericBot.DiscordClient.CurrentUser.Id) return;
                var msg =  GenericBot.DiscordClient.GetApplicationInfoAsync().Result.Owner.GetOrCreateDMChannelAsync().Result
                    .SendMessageAsync($"```\nDM from: {message.Author}({message.Author.Id})\nContent: {message.Content.SafeSubstring(1500)}\n```").Result;
                if (parameterMessage.Content.Split().Length == 1)
                {
                    var guild = VerificationEngine.GetGuildFromCode(parameterMessage.Content, message.Author.Id);
                    if (guild == null)
                    {
                        await message.ReplyAsync("Invalid verification code");
                    }
                    else
                    {
                        await guild.GetUser(message.Author.Id)
                            .AddRoleAsync(guild.GetRole(GenericBot.GuildConfigs[guild.Id].VerifiedRole));
                        if (GenericBot.GuildConfigs[guild.Id].UserJoinedShowModNotes && guild.TextChannels.HasElement(c =>c.Id == (GenericBot.GuildConfigs[guild.Id].UserLogChannelId), out SocketTextChannel logChannel))
                        {
                            logChannel.SendMessageAsync($"`{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm tt")}`:  `{message.Author}` (`{message.Author.Id}`) just verified");
                        }
                        await message.ReplyAsync($"You've been verified on **{guild.Name}**!");

                        await msg.ModifyAsync(m =>
                            m.Content = $"```\nDM from: {message.Author}({message.Author.Id})\nContent: {message.Content.SafeSubstring(1900)}\nVerified on {guild.Name}\n```");
                    }
                }
            }

            if (parameterMessage.Author.Id != GenericBot.DiscordClient.CurrentUser.Id &&
                GenericBot.GuildConfigs[parameterMessage.GetGuild().Id].FourChannelId == parameterMessage.Channel.Id)
            {

                await parameterMessage.DeleteAsync();
                await parameterMessage.ReplyAsync(
                    $"**[Anonymous]** {string.Format("{0:yyyy-MM-dd HH\\:mm\\:ss}", DateTimeOffset.UtcNow)}\n{parameterMessage.Content}");
            }

            GenericBot.QuickWatch.Restart();
            try
            {
                var commandInfo = CommandHandler.ParseMessage(parameterMessage);

                CustomCommand custom = new CustomCommand();

                if (parameterMessage.Channel is IDMChannel) goto DMChannel;

                if (GenericBot.GuildConfigs[parameterMessage.GetGuild().Id].CustomCommands
                        .HasElement(c => c.Name == commandInfo.Name, out custom) ||
                    GenericBot.GuildConfigs[parameterMessage.GetGuild().Id].CustomCommands
                        .HasElement(c => c.Aliases.Any(a => a.Equals(commandInfo.Name)), out custom))
                {
                    if (custom.Delete)
                    {
                        await parameterMessage.DeleteAsync();
                    }
                    await parameterMessage.ReplyAsync(custom.Response);
                }

                DMChannel:
                GenericBot.LastCommand = commandInfo;
                commandInfo.Command.ExecuteCommand(GenericBot.DiscordClient, message, commandInfo.Parameters).FireAndForget();
                GenericBot.CommandCounter++;
            }
            catch (NullReferenceException nullRefEx)
            {

            }
            catch (Exception ex)
            {
                if (parameterMessage.Author.Id == GenericBot.GlobalConfiguration.OwnerId)
                {
                    await parameterMessage.ReplyAsync("```\n" + $"{ex.Message}\n{ex.StackTrace}".SafeSubstring(1000) +
                                                      "\n```");
                }
                await GenericBot.Logger.LogErrorMessage(ex.Message);
                //else Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        public static async Task MessageDeleted(Cacheable<IMessage, ulong> arg,ISocketMessageChannel channel)
        {
            if (!arg.HasValue) return;

            var guildConfig = GenericBot.GuildConfigs[(arg.Value as SocketMessage).GetGuild().Id];

            if (guildConfig.UserLogChannelId == 0 || guildConfig.MessageLoggingIgnoreChannels.Contains(channel.Id)) return;

            EmbedBuilder log = new EmbedBuilder()
                .WithTitle("Message Deleted")
                .WithAuthor(new EmbedAuthorBuilder().WithName($"{arg.Value.Author} ({arg.Value.Author.Id})")
                    .WithIconUrl(arg.Value.Author.GetAvatarUrl() + " "))
                .WithColor(255, 0, 0)
                .WithCurrentTimestamp();

            log.AddField(new EmbedFieldBuilder().WithName("Channel").WithValue("#" + arg.Value.Channel.Name).WithIsInline(true));
            log.AddField(new EmbedFieldBuilder().WithName("Sent At").WithValue(arg.Value.Timestamp.ToString(@"yyyy-MM-dd HH:mm.ss") + "GMT").WithIsInline(true));


            if (!string.IsNullOrEmpty(arg.Value.Content))
            {
                log.WithDescription("**Message:** " + arg.Value.Content);
                foreach (var uid in arg.Value.MentionedUserIds)
                {
                    log.Description = log.Description.Replace($"<@!{uid}>", "@" + GenericBot.DiscordClient.GetUser(uid).Username);
                }
            }

            if (arg.Value.Attachments.Any())
            {
                log.AddField(new EmbedFieldBuilder().WithName("Attachments").WithValue(arg.Value.Attachments.Select(a =>
                    $"File: {a.Filename}").Aggregate((a, b) => a + "\n" + b)));
            }

            await (arg.Value as SocketMessage).GetGuild().GetTextChannel(guildConfig.UserLogChannelId).SendMessageAsync("", embed: log.Build());
        }

    }
}
