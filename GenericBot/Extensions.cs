using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenericBot
{
    public static class Extensions
    {
        public static async void FireAndForget(this Task task)
        {
            await task.ConfigureAwait(false);
        }
        public static List<string> SplitSafe(this string input)
        {
            List<string> output = new List<string>();
            var strings = input.Split(' ');

            string temp = "";
            foreach (var s in strings)
            {
                if (temp.Length + s.Length < 2000)
                {
                    temp += " " + s;
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
