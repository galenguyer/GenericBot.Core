using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Discord.Net.Queue;
using GenericBot.Entities;
using ThreadState = System.Diagnostics.ThreadState;

namespace GenericBot.CommandModules
{
    public class BotCommands
    {
        public List<Command> GetBotCommands()
        {
            List<Command> botCommands = new List<Command>();

            Command ping = new Command("ping");
            ping.Description = $"Get the ping time to the bot";
            ping.Usage = $"ping <verbose>";
            ping.RequiredPermission = Command.PermissionLevels.User;
            ping.ToExecute += async (client, msg, paramList) =>
            {
                var stop = new Stopwatch();
                stop.Start();
                var rep = await msg.Channel.SendMessageAsync("Pong!");

                if (paramList.FirstOrDefault() != null && paramList.FirstOrDefault().Equals("verbose"))
                {
                    stop.Stop();
                    await rep.ModifyAsync(m => m.Content = $"Pong! `{stop.ElapsedMilliseconds}ms`");
                }
            };
            botCommands.Add(ping);

            Command global = new Command("global");
            global.Description = "Get the global information for the bot";
            global.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            global.ToExecute += async (client, msg, paramList) =>
            {
                string stats = $"**Global Stats:** `{DateTime.Now}`\n" +
                               $"SessionID: `{GenericBot.Logger.SessionId}`\n\n" +
                               $"Servers: `{client.Guilds.Count}`\n" +
                               $"Users: `{client.Guilds.Sum(g => g.Users.Count)}`\n" +
                               $"Shards: `{client.Shards.Count}`\n" +
                               $"Memory: `{Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB`\n" +
                               $"Threads: `{Process.GetCurrentProcess().Threads.OfType<ProcessThread>().Count()}` " +
                               $"(`{Process.GetCurrentProcess().Threads.OfType<ProcessThread>().Count(t => t.ThreadState == ThreadState.Running)} Active`)\n" +
                               $"Uptime: `{(DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss")}\n`";
                await msg.Channel.SendMessageAsync(stats);
            };
            botCommands.Add(global);

            Command say = new Command("say");
            say.Delete = true;
            say.Aliases = new List<string>{"echo"};
            say.Description = "Say something a contributor said";
            say.SendTyping = true;
            say.Usage = "say <phrase>";
            say.ToExecute += async (client, msg, paramList) =>
            {
                await msg.Channel.SendMessageAsync(paramList.Aggregate((i, j) => i + " " + j));
            };

            botCommands.Add(say);

            return botCommands;
        }

        public List<Command> AddBotCommands(List<Command> preCommands)
        {
            preCommands.AddRange(GetBotCommands());
            return preCommands;
        }
    }
}
