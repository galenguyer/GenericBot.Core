using System;
using Discord.WebSocket;

namespace GenericBot
{
    public static class VerificationEngine
    {
        public static string GetVerificationCode(ulong userId, ulong guildId)
        {
            var pid = int.Parse(userId.ToString().Substring(7, 6));
            var gid = int.Parse(guildId.ToString().Substring(7, 6));

            return (gid + pid).ToString("X").ToLower();
        }

        public static SocketGuild GetGuildFromCode(string code, ulong userId)
        {
            var pid = int.Parse(userId.ToString().Substring(7, 6));
            var sum = Convert.ToInt32(code, 16);
            var gid = sum - pid;

            if (GenericBot.DiscordClient.Guilds.HasElement(g => g.Id.ToString().Contains(gid.ToString()),
                out SocketGuild guild))
                return guild;
            return null;
        }

        public static string InsertCodeInMessage(string message, string code)
        {
            int wc = message.Length;

            int sPos = new Random().Next((wc/2), wc);
            for (int i = sPos; i < wc; i++)
            {
                if (message[i].Equals(' '))
                    break;
                sPos++;
            }

            message = message.Substring(0, sPos) + $" *(the secret is: {code})* " + message.Substring(sPos);

            return message;
        }
    }
}
