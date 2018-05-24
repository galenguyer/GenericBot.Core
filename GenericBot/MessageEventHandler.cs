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
                    .SendMessageAsync($"```\nDM from: {message.Author}({message.Author.Id})\nContent: {message.Content.SafeSubstring(1900)}\n```").Result;
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


            string logMessage = $"```diff\n- Message DELETED by {arg.Value.Author} ({arg.Value.Author.Id})\nat {DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm.ss")} GMT" +
                                $" (Sent at {arg.Value.Timestamp.ToString(@"yyyy-MM-dd HH:mm.ss")} GMT)\nin #{arg.Value.Channel.Name.TrimStart('#')}\n";

            logMessage += $"Content: {arg.Value.Content.Replace('`', '\'').SafeSubstring(1650)}";

            if(arg.Value.Attachments.Any()){
                foreach (var a in arg.Value.Attachments)
                {
                    logMessage += $"\nFile: {a.Filename}";
                }
            }
            foreach (var uid in arg.Value.MentionedUserIds)
            {
                logMessage = logMessage.Replace($"<@!{uid}>", "@" + GenericBot.DiscordClient.GetUser(uid).Username);
            }

            logMessage += "\n```";

            (arg.Value as SocketMessage).GetGuild().GetTextChannel(guildConfig.UserLogChannelId).SendMessageAsync(logMessage);
        }

    }
}
