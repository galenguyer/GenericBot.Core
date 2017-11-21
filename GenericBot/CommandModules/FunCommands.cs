using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Net.Queue;
using GenericBot.Entities;
using MarkVSharp;

namespace GenericBot.CommandModules
{
    public class FunCommands
    {
        public List<Command> GetFunCommands()
        {
            List<Command> FunCommands = new List<Command>();

            Command wat = new Command("wat");
            wat.Description = "The best command";
            wat.ToExecute += async (client, msg, parameters) =>
            {
                await msg.ReplyAsync($"**-wat-**\nhttp://destroyallsoftware.com/talks/wat");
            };

            FunCommands.Add(wat);

            Command markov = new Command("markov");
            markov.Description = "Create a markov chain from the last messages in the channel";
            markov.Delete = true;
            markov.Usage = "markov";
            markov.ToExecute += async (client, msg, parameters) =>
            {
                var messages = msg.Channel.GetMessagesAsync().Flatten().Result.Reverse().Select(m =>m.Content).ToList();
                messages.ToList().AddRange(messages.TakeLast(50));
                messages.ToList().AddRange(messages.TakeLast(25));
                messages.ToList().AddRange(messages.TakeLast(10));

                int averageLength = messages.Sum(m => m.Split(' ').Length) / 185;
                averageLength = averageLength > 10 ? averageLength : averageLength * 2;

                var markovGenerator = new MarkovGenerator(messages.Aggregate((i, j) => i.TrimEnd('.') + ". " + j));

                await msg.ReplyAsync(markovGenerator.GenerateSentence(averageLength));
            };

            FunCommands.Add(markov);

            return FunCommands;
        }
    }
}
