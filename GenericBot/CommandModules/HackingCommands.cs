using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class HackingCommands
    {
        public List<Command> GetHackedCommands()
        {
            List<Command> HackedCommands = new List<Command>();

            Command user = new Command("user");
            user.RequiredPermission = Command.PermissionLevels.BotOwner;
            user.Usage = "user <init|kill>";
            user.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters[0].ToLower() == "init")
                {
                    if (UserAccount.Client.ConnectionState == ConnectionState.Connected)
                    {
                        await msg.ReplyAsync("Already initiated");
                        return;
                    }
                    else
                    {
                        UserAccount.Client.Log += async (message) =>
                        {
                            await msg.ReplyAsync($"[UserLog] `{message}`");
                        };
                        UserAccount.SetToken(File.ReadAllText("files/usertoken"));
                        await UserAccount.Connect();

                    }
                }
                else if (parameters[0].ToLower() == "kill")
                {
                    if (UserAccount.Client.ConnectionState == ConnectionState.Connected)
                    {
                        await UserAccount.Client.LogoutAsync();
                        UserAccount.Client.Log += null;
                    }
                    else await msg.ReplyAsync($"Not initiated");
                }
                else if (parameters[0].ToLower() == "mutualusers")
                {
                    if (UserAccount.Client.ConnectionState != ConnectionState.Connected)
                    {
                        await msg.ReplyAsync($"Not Connected (`{UserAccount.Client.ConnectionState}`)");
                    }
                    else
                    {
                        if (UserAccount.Client.Guilds.All(g => g.Id.ToString() != parameters[1]))
                        {
                            await msg.ReplyAsync($"Guild (`{parameters[1]}`) not found");
                        }
                        else
                        {
                            await msg.ReplyAsync($"Found Guild `{UserAccount.Client.GetGuild(ulong.Parse(parameters[1])).Name}`");
                            var extUsers = UserAccount.Client.GetGuild(ulong.Parse(parameters[1])).Users.ToList();
                            var guildUsers = msg.GetGuild().Users.ToList();

                            var inter = extUsers.Where(u => guildUsers.Select(gu => gu.Id).Contains(u.Id));

                            string mutual = "Mutual users: \n";
                            foreach (var u in inter)
                            {
                                mutual += $"{u} (`{u.Id}`)\n";
                            }

                            foreach (var s in mutual.SplitSafe('\n'))
                            {
                                await msg.ReplyAsync(s);
                            }
                        }
                    }
                }
                else
                {
                    await msg.ReplyAsync("Option Not Found");
                }

            };

            HackedCommands.Add(user);

            return HackedCommands;
        }
    }

    public static class UserAccount
    {
        public static DiscordSocketClient Client = new DiscordSocketClient(new DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            LogLevel = LogSeverity.Info
        });
        private static string token { get;  set; }

        public static async Task Connect()
        {

            await Client.LoginAsync(TokenType.User, token);
            await Client.StartAsync();


            await Task.Delay(-1);
        }

        public static void SetToken(string t)
        {
            token = t;
        }
    }
}
