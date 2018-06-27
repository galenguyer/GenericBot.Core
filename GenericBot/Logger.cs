using System;
using System.IO;
using System.Threading.Tasks;
using Discord;

namespace GenericBot
{
    public class Logger
    {
        public readonly string SessionId;

        public Logger(string sId)
        {
            SessionId = sId;
            Directory.CreateDirectory("files");
            Directory.CreateDirectory("files/sessions");
            LogGenericMessage($"New Logger created with SessionID of {SessionId}");
        }

        public Task LogClientMessage(LogMessage msg)
        {
            string message = $"[{msg.Severity}] {DateTime.Now}: {msg.Message}";
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
            File.AppendAllText($"files/sessions/{SessionId.Substring(0, 8)}.log", message + "\n");
            return Task.FromResult(1);
        }

        public Task LogGenericMessage(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            string message = $"[Generic] {DateTime.Now}: {msg}";
            Console.WriteLine(message);
            File.AppendAllText($"files/sessions/{SessionId.Substring(0, 8)}.log", message + "\n");
            return Task.FromResult(1);
        }
        public Task LogErrorMessage(string msg)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            string message = $"[Error] {DateTime.Now}: {msg}";
            Console.WriteLine(message);
            File.AppendAllText($"files/sessions/{SessionId.Substring(0, 8)}.log", message + "\n");
            return Task.FromResult(1);
        }
    }
}