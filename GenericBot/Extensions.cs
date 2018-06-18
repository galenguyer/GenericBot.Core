using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static string Escape(this string input)
        {
            var result = input;
            result = result.Replace(@"\", @"\\");
            result = result.Replace(@"*", @"\*");
            result = result.Replace(@"_", @"\_");
            result = result.Replace(@"~", @"\~");
            result = result.Replace(@"`", @"\`");
       
            return result;
        }

        public static string GetDisplayName(this SocketGuildUser user)
        {
            if (string.IsNullOrEmpty(user.Nickname))
                return user.Username;

            return user.Nickname;
        }

        public static SocketGuild GetGuild(this SocketMessage msg)
        {
            return ((SocketGuildChannel) msg.Channel).Guild;
        }

        public static Task<RestUserMessage> ReplyAsync(this SocketMessage msg, object text)
        {
            return msg.Channel.SendMessageAsync(text.ToString().Replace("@everyone", "@-everyone")
                .Replace("@here", "@-here"));
        }

        public static bool Empty(this List<string> list)
        {
            if (list == null) return true;
            return list.All(i => string.IsNullOrEmpty(i.Trim()));
        }

        public static string reJoin(this List<string> list, string joinChar = " ")
        {
            if (list.Count == 0) return "";
            return list.Aggregate((i, j) => i + joinChar + j);
        }

        public static List<SocketGuildUser> GetMentionedUsers(this SocketMessage msg)
        {
            var users = new HashSet<SocketGuildUser>();
            if (msg.MentionedUsers.Any())
            {
                foreach (var user in msg.MentionedUsers)
                    users.Add((SocketGuildUser) user);
            }

            foreach (Match match in Regex.Matches(msg.Content, "[0-9]{16,19}"))
            {
                users.Add((SocketGuildUser) msg.GetGuild().GetUser(Convert.ToUInt64(match.Value)));
            }

            return users.ToList();
        }

        public static bool HasElement<T>(this IEnumerable<T> inEnum, Func<T, bool> predicate, out T output)
        {
            try
            {
                inEnum = inEnum.ToList();
                if (inEnum != null && inEnum.Any() && inEnum.Any(predicate))
                {
                    output = inEnum.First(predicate);
                    return true;
                }

                output = default(T);
                return false;
            }
            catch (Exception e)
            {
                output = default(T);
                return false;
            }
        }

        public static bool Override<T, O>(this Dictionary<T, O> dict, T key, O value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
                return true;
            }
            else
            {
                dict.Add(key, value);
                return false;
            }
        }

        public static async Task<List<IMessage>> GetManyMessages(this SocketTextChannel channel, int count)
        {
            count++;
            var msgs = (channel as IMessageChannel).GetMessagesAsync().FlattenAsync().Result;
            await Task.Yield();

            while (true)
            {
                var newmsgs = (channel as IMessageChannel).GetMessagesAsync(msgs.Last(), Direction.Before)
                    .FlattenAsync().Result;
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
                if (temp.Length + s.Length < 1800)
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

        public static string SumAnd<T>(this List<T> input)
         {
             if (!input.Any())
             {
                 return "";
             }
             else if (input.Count == 1)
             {
                 return input.First().ToString();
             }
             else if (input.Count == 2)
             {
                 return $"{input.First()} and {input.Last()}";
             }
             else
             {
                 var newIN = new List<T>();
                  newIN.AddRange(input);

                 return SumAndPriv(newIN, "");
             }
         }

        private static string SumAndPriv<T>(List<T> input, string previous)
        {
            if (!input.Any())
            {
                return "";
            }
            else if (input.Count == 1)
            {
                return input.First().ToString();
            }
            else if (input.Count == 2)
            {
                return $"{previous}, {input.First()}, and {input.Last()}";
            }
            else
            {
                string first = input.First().ToString();
                if (!string.IsNullOrEmpty(previous))
                {
                    previous = $"{previous}, {first}";
                }
                else
                {
                    previous = first;
                }
                input.RemoveAt(0);
                return SumAndPriv<T>(input, previous);
            }
        }

        public static string Nice(this TimeSpan time)
        {
            if (Math.Floor(time.TotalDays) > 0)
            {
                return $"{Math.Floor(time.TotalDays)} days";
            }
            else if (Math.Floor(time.TotalDays) > 0)
            {
                return $"{Math.Floor(time.TotalDays)} days {Math.Floor((double) time.Hours)} hours";
            }
            else if (Math.Floor(time.TotalHours) > 0) //Days = 0
            {
                return $"{Math.Floor(time.TotalHours)} hours {Math.Floor((double) time.Minutes)} minutes";
            }
            else //Days = 0 && Hours = 0
            {
                return $"{Math.Floor(time.TotalMinutes)} minutes {Math.Floor((double) time.Seconds)} seconds";
            }
        }

        public static string Multiply(this char str, int count)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                builder.Append(str);
            }

            return builder.ToString();
        }
    }
}
