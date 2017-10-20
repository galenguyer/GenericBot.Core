using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            return (msg.Channel as SocketGuildChannel).Guild;
        }

        public static Task<RestUserMessage> ReplyAsync(this SocketMessage msg, string text)
        {
            return msg.Channel.SendMessageAsync(text);
        }

        public static bool Empty(this List<string> list)
        {
            return list.All(i => string.IsNullOrEmpty(i.Trim()));
        }

        public static string reJoin(this List<string> list)
        {
            return list.Aggregate((i, j) => i + " " + j);
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
    }
}
