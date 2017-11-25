using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace GenericBot
{
    public static class Extensions
    {
        public static async void FireAndForget(this Task task)
        {
            await task.ConfigureAwait(false);
        }

        public static SocketGuild GetGuild(this SocketMessage msg)
        {
            return ((SocketGuildChannel) msg.Channel).Guild;
        }

        public static Task<RestUserMessage> ReplyAsync(this SocketMessage msg, object text)
        {
            return msg.Channel.SendMessageAsync(text.ToString().Replace("@everyone", "@-everyone").Replace("@here", "@-here"));
        }

        public static bool Empty(this List<string> list)
        {
            return list.All(i => string.IsNullOrEmpty(i.Trim()));
        }

        public static string reJoin(this List<string> list, string joinChar = " ")
        {
            return list.Aggregate((i, j) => i + joinChar + j);
        }

        public static List<SocketUser> GetMentionedUsers(this SocketMessage msg)
        {
            var users = msg.MentionedUsers.ToList();

            var allUsers = GenericBot.DiscordClient.Guilds.SelectMany(g => g.Users);

            foreach (Match match in Regex.Matches(msg.Content, "[0-9]{17,18}"))
            {
                if (allUsers.Any(u => u.Id.ToString() == match.Value) &&
                    users.All(u => u.Id.ToString() != match.Value))
                {
                    users.Add(GenericBot.DiscordClient.GetUser(Convert.ToUInt64(match.Value)));
                }
            }
            return users.ToList();
        }

        public static bool HasElement<T>(this IEnumerable<T> inEnum, Func<T, bool> predicate, out T output)
        {
            if (inEnum.Any(predicate))
            {
                output = inEnum.First(predicate);
                return true;
            }
            output = default(T);
            return false;
        }

        public static async Task<List<IMessage>> GetManyMessages(this SocketTextChannel channel, int count)
        {
            count++;
            var msgs = (channel as IMessageChannel).GetMessagesAsync().Flatten().Result;
            await Task.Yield();

            while (true)
            {
                var newmsgs = (channel as IMessageChannel).GetMessagesAsync(msgs.Last(), Direction.Before).Flatten().Result;
                msgs = msgs.Concat(newmsgs);
                await Task.Yield();
                if (newmsgs.Count() < 100 || msgs.Count() > count) break;
            }

            return msgs.Distinct().Take(count).ToList();
        }

        public static ulong GetRandomItem(this List<ulong> list)
        {
            return list[new Random().Next(0, list.Count - 1)];
        }

        public static List<string> SplitSafe(this string input, char spl = ' ')
        {
            List<string> output = new List<string>();
            var strings = input.Split(spl);

            string temp = "";
            foreach (var s in strings)
            {
                if (temp.Length + s.Length < 2000)
                {
                    temp += spl + s;
                }
                else
                {
                    output.Add(temp);
                    temp = s;
                }
            }
            output.Add(temp);

            return output;
        }

        public static string SafeSubstring(this string input, int length)
        {
            if (input.Length < length)
            {
                return input;
            }
            else return input.Substring(0, length) + "...";
        }
    }
}
