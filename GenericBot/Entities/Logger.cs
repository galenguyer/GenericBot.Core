using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace GenericBot
{
    public class Logger
    {
        public readonly string SessionId;

        public Logger()
        {
            SessionId = $"{DateTime.UtcNow.ToString(@"yyyy-MM-dd_HH-mm")}_{new Random().Next(1000, 9999)}";
            Directory.CreateDirectory("./files/sessions");
            LogGenericMessage($"New Logger created with SessionID of {SessionId}");
        }

        public Task LogClientMessage(LogMessage msg)
        {
            string message = $"[{msg.Severity}] {DateTime.UtcNow.ToString(@"yyyy-MM-dd_HH-mm")}: {msg.Message}";
            if (msg.Severity != LogSeverity.Debug)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(message);
            }
            File.AppendAllText($"files/sessions/{SessionId}.log", message + "\n");
            return Task.FromResult(1);
        }

        public Task LogGenericMessage(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            string message = $"[Generic] {DateTime.UtcNow.ToString(@"yyyy-MM-dd_HH-mm")}: {msg}";
            Console.WriteLine(message);
            File.AppendAllText($"files/sessions/{SessionId.Substring(0, 8)}.log", message + "\n");
            return Task.FromResult(1);
        }
        public Task LogErrorMessage(Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            string message = $"[Error] {DateTime.UtcNow.ToString(@"yyyy-MM-dd_HH-mm")}: {exception}";
            Console.WriteLine(message);
            File.AppendAllText($"files/sessions/{SessionId.Substring(0, 8)}.log", message + "\n");

            if (!string.IsNullOrEmpty(Core.GlobalConfig.CriticalLoggingWebhookUrl))
            {
                var webhook = new Discord.Webhook.DiscordWebhookClient(Core.GlobalConfig.CriticalLoggingWebhookUrl);
                var builder = new EmbedBuilder()
                    .WithColor(255, 0, 0)
                    .WithCurrentTimestamp()
                    .AddField(new EmbedFieldBuilder()
                        .WithName("Error Message")
                        .WithValue(exception.Message))
                    .AddField(new EmbedFieldBuilder()
                        .WithName("Stack Trace")
                        .WithValue(exception.StackTrace));
                webhook.SendMessageAsync("", embeds: new List<Embed> { builder.Build() });
            }

            return Task.FromResult(1);
        }
    }
}