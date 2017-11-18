using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net.Queue;
using Discord.Rest;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class ModCommands
    {
        public List<Command> GetModCommands()
        {
            List<Command> ModCommands = new List<Command>();

            Command clear = new Command("clear");
            clear.Description = "Clear a number of messages from a channel";
            clear.Usage = "clear <number> <user>";
            clear.RequiredPermission = Command.PermissionLevels.Moderator;
            clear.ToExecute += async (client, msg, paramList) =>
            {
                if (paramList.Empty())
                {
                    await msg.ReplyAsync("You gotta tell me how many messages to delete!");
                    return;
                }

                int count;
                if (int.TryParse(paramList[0], out count))
                {
                    List<IMessage> msgs = (msg.Channel as SocketTextChannel).GetManyMessages(count).Result;
                    if (msg.GetMentionedUsers().Any())
                    {
                        var users = msg.GetMentionedUsers();
                        msgs = msgs.Where(m => users.Select(u => u.Id).Contains(m.Author.Id)).ToList();
                        msgs.Add(msg);
                    }

                    await msg.Channel.DeleteMessagesAsync(msgs.Where(m => DateTime.Now - m.CreatedAt < TimeSpan.FromDays(14)));

                    var messagesSent = new List<IMessage>();

                    messagesSent.Add(msg.ReplyAsync($"{msg.Author.Mention}, done deleting those messages!").Result);
                    if (msgs.Any(m => DateTime.Now - m.CreatedAt > TimeSpan.FromDays(14)))
                    {
                        messagesSent.Add(msg.ReplyAsync($"I couldn't delete all of them, some were older than 2 weeks old :frowning:").Result);
                    }

                    await Task.Delay(2500);
                    await msg.Channel.DeleteMessagesAsync(messagesSent);
                }
                else
                {
                    await msg.ReplyAsync("That's not a valid number");
                }
            };

            Command archive = new Command("archive");
            archive.RequiredPermission = Command.PermissionLevels.Admin;
            archive.Description = "Save all the messages from a text channel";
            archive.ToExecute += async (client, msg, parameters) =>
            {
                var msgs = (msg.Channel as SocketTextChannel).GetManyMessages(50000).Result;

                var channel = msg.Channel;
                string str = $"{((IGuildChannel) channel).Guild.Name} | {((IGuildChannel) channel).Guild.Id}\n";
                str += $"#{channel.Name} | {channel.Id}\n";
                str += $"{DateTime.Now}\n\n";

                IMessage lastMsg = null;
                msgs.Reverse();
                msgs.Remove(msg);
                foreach (var m in msgs)
                {
                    string msgstr = "";
                    if(lastMsg != null && m.Author.Id != lastMsg.Author.Id) msgstr += $"{m.Author} | {m.Author.Id}\n";
                    if (lastMsg != null && m.Author.Id != lastMsg.Author.Id) msgstr += $"{m.Timestamp}\n";
                    msgstr += $"{m.Content}\n";
                    foreach (var a in m.Attachments)
                    {
                        msgstr += $"{a.Url}\n";
                    }
                    str += msgstr + "\n";
                    lastMsg = m;
                    await Task.Yield();
                }

                string filename = $"{channel.Name}.txt";
                File.WriteAllText("files/" + filename, str);
                await msg.Channel.SendFileAsync("files/" + filename, $"Here you go! I saved {msgs.Count()} messages");
            };

            ModCommands.Add(archive);

            ModCommands.Add(clear);

            return ModCommands;
        }
    }
}
