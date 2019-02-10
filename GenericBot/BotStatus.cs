using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace GenericBot
{
    public class BotStatus
    {
        //public int GuildCount { get; set; }
        //public int UserCount { get; set; }
        //public string Uptime { get; set; }
        //public int MessageCounter { get; set; }
        //public int CommandCounter { get; set; }
        //public int Latency { get; set; }
        //public string ServerInfo { get; set; }
        //public string MemoryUsage { get; set; }

        //public BotStatus()
        //{
        //    GuildCount = GenericBot.DiscordClient.Guilds.Count;
        //    UserCount = GenericBot.DiscordClient.Guilds.Sum(g => g.Users.Count);
        //    Uptime = (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        //    MessageCounter = GenericBot.MessageCounter;
        //    CommandCounter = GenericBot.CommandCounter;
        //    Latency = GenericBot.Latency;
        //    this.MemoryUsage = $"{Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB";
        //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        //    {
        //        var process = new Process()
        //        {
        //            StartInfo = new ProcessStartInfo
        //            {
        //                FileName = "/bin/bash",
        //                Arguments = $"-c \"cat /etc/*-release | grep DESCRIPTION | cut -d '=' -f2",
        //                RedirectStandardOutput = true,
        //                UseShellExecute = false,
        //                CreateNoWindow = true,
        //            }
        //        };
        //        process.Start();
        //        var result = process.StandardOutput.ReadToEnd();
        //        process.WaitForExit();
        //        this.ServerInfo = result;
        //    }
        //}
    }
}
