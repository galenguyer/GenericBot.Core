using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Discord;
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
                var messages = msg.Channel.GetMessagesAsync().FlattenAsync().Result.Reverse().Select(m =>m.Content).ToList();
                messages.ToList().AddRange(messages.TakeLast(50));
                messages.ToList().AddRange(messages.TakeLast(25));
                messages.ToList().AddRange(messages.TakeLast(10));

                int averageLength = messages.Sum(m => m.Split(' ').Length) / 185;
                averageLength = averageLength > 10 ? averageLength : averageLength * 2;

                var markovGenerator = new MarkovGenerator(messages.Aggregate((i, j) => i.TrimEnd('.') + ". " + j));

                await msg.ReplyAsync(markovGenerator.GenerateSentence(averageLength));
            };

            FunCommands.Add(markov);

            Command roll = new Command("roll");
            roll.Aliases.Add("dice");
            roll.Description = "Roll a specified number of dices. Defaults to 1d20 if no parameters";
            roll.Usage = "roll [count]d[sides]";
            roll.ToExecute += async (client, msg, parameters) =>
            {
                uint count = 1;
                uint sides = 20;
                int add = 0;
                if(!parameters.Empty())
                {
                    string param = parameters.reJoin("").ToLower();
                    if (!param.Contains("d"))
                    {
                        if (!uint.TryParse(param, out count))
                        {
                            await msg.ReplyAsync("Input improperly formatted");
                            return;
                        }
                    }
                    else
                    {
                        if (!(param.Contains("+") || param.Contains("-")))
                        {
                            var list = param.Split('d');
                            if (!(uint.TryParse(list[0], out count) && uint.TryParse(list[1], out sides)))
                            {
                                await msg.ReplyAsync("Input improperly formatted");
                                return;
                            }
                        }
                        else
                        {
                            string c = param.Split('d')[0];
                            string second = param.Split('d')[1];
                            string s = "";
                            string a = "";
                            if (param.Contains("+"))
                            {
                                s = second.Split('+')[0];
                                a = second.Split('+')[1];
                            }
                            else if (param.Contains("-"))
                            {
                                s = second.Split('-')[0];
                                a = second.Replace(s, "");
                            }

                            if(!(uint.TryParse(c, out count) && uint.TryParse(s, out sides) && int.TryParse(a, out add)))
                            {
                                await msg.ReplyAsync("Input improperly formatted");
                                return;
                            }
                        }
                    }
                }

                if (count > 100)
                {
                    await msg.ReplyAsync("I don't think you can hold that many!");
                    return;
                }
                if (sides < 2)
                {
                    await msg.ReplyAsync("You can't have a 1-sided dice!");
                    return;
                }
                if (sides > 100)
                {
                    await msg.ReplyAsync("That's an awefully large dice, I can't hold that");
                    return;
                }
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                List<int> results = new List<int>();
                while(results.Count < count)
                {
                    byte[] bytes = new byte[4];
                    crypto.GetNonZeroBytes(bytes);
                    long rand = Math.Abs(BitConverter.ToInt32(bytes, 0)) % sides;
                    results.Add((int) rand + 1 + add);
                }

                string res = $"{msg.Author.Mention}, you rolled ";
                results.Sort();
                res += results.SumAnd();
                if(count > 1) res += $" with a total of {results.Sum()}";
                await msg.ReplyAsync(res);
            };

            FunCommands.Add(roll);

            return FunCommands;
        }
    }
}
