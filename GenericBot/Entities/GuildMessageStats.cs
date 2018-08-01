using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GenericBot.Entities
{
    class GuildMessageStats
    {
        public class StatsYear
        {
            public int Year;
            public List<StatsMonth> Months = new List<StatsMonth>();

            public StatsYear(int year, int month, int day, ulong userId)
            {
                this.Year = year;
                this.Months = new List<StatsMonth> { new StatsMonth(month, day, userId) };
            }
        }
        public class StatsMonth
        {
            public int Month;
            public List<StatsDay> Days = new List<StatsDay>();

            public StatsMonth(int month, int day, ulong userId)
            {
                this.Month = month;
                this.Days = new List<StatsDay> { new StatsDay(day, userId) };
            }
        }
        public class StatsDay
        {
            public int Day;
            public List<StatsUser> Users = new List<StatsUser>();

            public StatsDay(int day, ulong userId)
            {
                this.Day = day;
                this.Users = new List<StatsUser> { new StatsUser(userId) };
            }
        }
        public class StatsUser
        {
            public ulong UserId;
            public int MessageCount;
            public Dictionary<string, int> Commands;

            public StatsUser(ulong userId)
            {
                this.UserId = userId;
                this.MessageCount = 1;
                this.Commands = new Dictionary<string, int>();
            }
        }

        public List<StatsYear> Years = new List<StatsYear>();
        public ulong GuildId;

        public GuildMessageStats()
        {

        }
        public GuildMessageStats(ulong guildId)
        {
            while (GenericBot.LockedFiles.Contains($"files/guildStats/{guildId}.json"))
            {
                //wait
            }
            GenericBot.LockedFiles.Add($"files/guildStats/{guildId}.json");

            if (File.Exists($"files/guildStats/{guildId}.json"))
            {
                var tmp = JsonConvert.DeserializeObject<GuildMessageStats>(File.ReadAllText($"files/guildStats/{guildId}.json"));
                this.GuildId = tmp.GuildId;
                this.Years = tmp.Years;
            }
            else
            {
                this.GuildId = guildId;
                this.Years = new List<StatsYear>(); 
            }
        }

        public void Save()
        {
            Directory.CreateDirectory("files");
            Directory.CreateDirectory("files/guildStats");
            File.WriteAllText($"files/guildStats/{this.GuildId}.json", JsonConvert.SerializeObject(this, Formatting.Indented));
            GenericBot.LockedFiles.Remove($"files/guildStats/{this.GuildId}.json");
        }

        /// <summary>
        /// This is a promise not to modify the returned object. Doing so strongly risks a race condition
        /// </summary>
        /// <returns></returns>
        public GuildMessageStats DisposeLoader()
        {
            GenericBot.LockedFiles.Remove($"files/guildStats/{this.GuildId}.json");
            return this;
        }

        public GuildMessageStats AddMessage(ulong uId)
        {
            var now = DateTimeOffset.UtcNow;
            if(Years.HasElement(y=> y.Year.Equals(now.Year), out var year))
            {
                if (year.Months.HasElement(m => m.Month.Equals(now.Month), out var month))
                {
                    if(month.Days.HasElement(d => d.Day.Equals(now.Day), out var day))
                    {
                        if (day.Users.HasElement(u => u.UserId.Equals(uId), out var user))
                        {
                            user.MessageCount++;
                        }
                        else
                        {
                            day.Users.Add(new StatsUser(uId));
                        }
                    }
                    else
                    {
                        month.Days.Add(new StatsDay(now.Day, uId));
                    }
                }
                else
                {
                    year.Months.Add(new StatsMonth(now.Month, now.Day, uId));
                }
            }
            else
            {
                this.Years.Add(new StatsYear(now.Year, now.Month, now.Day, uId));
            }
            return this;
        }

        public GuildMessageStats AddCommand(ulong uId, string command)
        {
            command = command.ToLower();
            var now = DateTimeOffset.UtcNow;
            if (Years.HasElement(y => y.Year.Equals(now.Year), out var year))
            {
                if (year.Months.HasElement(m => m.Month.Equals(now.Month), out var month))
                {
                    if (month.Days.HasElement(d => d.Day.Equals(now.Day), out var day))
                    {
                        if (day.Users.HasElement(u => u.UserId.Equals(uId), out var user))
                        {
                            if (user.Commands.Any(k => k.Key.Equals(command)))
                            {
                                user.Commands[command]++;
                            }
                            else
                            {
                                user.Commands.Add(command, 1);
                            }
                        }
                        else
                        {
                            day.Users.Add(new StatsUser(uId));
                            var u = day.Users.First(us => us.UserId == uId);
                            if (u.Commands.Any(k => k.Key.Equals(command)))
                            {
                                u.Commands[command]++;
                            }
                            else
                            {
                                u.Commands.Add(command, 1);
                            }
                        }
                    }
                    else
                    {
                        month.Days.Add(new StatsDay(now.Day, uId));
                        var user = month.Days.First(d => d.Day == now.Day)
                            .Users.First(u => u.UserId == uId);
                        if (user.Commands.Any(k => k.Key.Equals(command)))
                        {
                            user.Commands[command]++;
                        }
                        else
                        {
                            user.Commands.Add(command, 1);
                        }
                    }
                }
                else
                {
                    year.Months.Add(new StatsMonth(now.Month, now.Day, uId));
                    var user = year.Months.First(m => m.Month == now.Month)
                        .Days.First(d => d.Day == now.Day)
                        .Users.First(u => u.UserId == uId);
                    if (user.Commands.Any(k => k.Key.Equals(command)))
                    {
                        user.Commands[command]++;
                    }
                    else
                    {
                        user.Commands.Add(command, 1);
                    }
                }
            }
            else
            {
                this.Years.Add(new StatsYear(now.Year, now.Month, now.Day, uId));
                var user = this.Years.First(y => y.Year == now.Year)
                    .Months.First(m => m.Month == now.Month)
                    .Days.First(d => d.Day == now.Day)
                    .Users.First(u => u.UserId == uId);
                if (user.Commands.Any(k => k.Key.Equals(command)))
                {
                    user.Commands[command]++;
                }
                else
                {
                    user.Commands.Add(command, 1);
                }
            }
            return this;
        }

    }
}
