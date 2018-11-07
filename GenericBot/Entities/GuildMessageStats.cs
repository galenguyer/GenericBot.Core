using LiteDB;
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
        public class StatsUser
        {
            public ulong Id;
            public List<StatsYear> Years = new List<StatsYear>();

            public StatsUser(ulong userId, int year, int month, int day)
            {
                this.Id = userId;
                this.Years = new List<StatsYear> { new StatsYear(year, month, day) };
            }
        }
        public class StatsYear
        {
            public int Year;
            public List<StatsMonth> Months = new List<StatsMonth>();

            public StatsYear(int year, int month, int day)
            {
                this.Year = year;
                this.Months = new List<StatsMonth> { new StatsMonth(month, day) };
            }
        }
        public class StatsMonth
        {
            public int Month;
            public List<StatsDay> Days = new List<StatsDay>();

            public StatsMonth(int month, int day)
            {
                this.Month = month;
                this.Days = new List<StatsDay> { new StatsDay(day) };
            }
        }
        public class StatsDay
        {
            public int Day;
            public int MessageCount;
            public Dictionary<string, int> Commands = new Dictionary<string, int>();

            public StatsDay(int day)
            {
                this.Day = day;
                MessageCount = 1;
            }
        }

        public List<StatsUser> Users = new List<StatsUser>();
        [BsonId] public ulong ID { get; set; }

        public GuildMessageStats()
        {

        }
        public GuildMessageStats(ulong guildId)
        {
            this.ID = guildId;
            try
            {
                if (GenericBot.LoadedGuildMessageStats.ContainsKey(this.ID))
                {
                    this.Users = GenericBot.LoadedGuildMessageStats[this.ID].Users;
                }
                else
                {
                    var col = GenericBot.GlobalDatabase.GetCollection<GuildMessageStats>("messageStats");
                    GuildMessageStats tempdb;
                    col.EnsureIndex(c => c.ID, true);
                    if (col.Exists(c => c.ID.Equals(guildId)))
                    {
                        tempdb = col.FindOne(c => c.ID.Equals(guildId));
                    }
                    else
                    {
                        tempdb = new GuildMessageStats() { ID = guildId, Users = new List<StatsUser>() };
                    }
                    this.Users = tempdb.Users;
                }
            }
            catch (Exception ex)
            {
                GenericBot.Logger.LogErrorMessage($"Load Stats for {guildId} Failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void Save()
        {
            try
            {
                GenericBot.LoadedGuildMessageStats[this.ID] = this;
                var col = GenericBot.GlobalDatabase.GetCollection<GuildMessageStats>("messageStats");
                col.EnsureIndex(c => c.ID, true);
                col.Upsert(this);
            }
            catch (Exception ex)
            {
                GenericBot.Logger.LogErrorMessage($"GuildID: {this.ID}\n{ex.Message}\n{ex.StackTrace}");
                this.Save();
            }

        }


        public GuildMessageStats AddMessage(ulong uId)
        {
            var now = DateTimeOffset.UtcNow;
            if (Users.HasElement(u => u.Id.Equals(uId), out var user))
            {
                if (user.Years.HasElement(y => y.Year.Equals(now.Year), out var year))
                {
                    if (year.Months.HasElement(m => m.Month.Equals(now.Month), out var month))
                    {
                        if (month.Days.HasElement(d => d.Day.Equals(now.Day), out var day))
                        {
                            day.MessageCount++;
                        }
                        else
                        {
                            month.Days.Add(new StatsDay(now.Day));
                        }
                    }
                    else
                    {
                        year.Months.Add(new StatsMonth(now.Month, now.Day));
                    }
                }
                else
                {
                    user.Years.Add(new StatsYear(now.Year, now.Month, now.Day));
                }
            }
            else
            {
                this.Users.Add(new StatsUser(uId, now.Year, now.Month, now.Day));
            }
            return this;
        }

        public GuildMessageStats AddCommand(ulong uId, string command)
        {
            command = command.ToLower();
            var now = DateTimeOffset.UtcNow;
            if (Users.HasElement(u => u.Id.Equals(uId), out var user))
            {
                if (user.Years.HasElement(y => y.Year.Equals(now.Year), out var year))
                {
                    if (year.Months.HasElement(m => m.Month.Equals(now.Month), out var month))
                    {
                        if (month.Days.HasElement(d => d.Day.Equals(now.Day), out var day))
                        {
                            if (day.Commands.Any(k => k.Key.Equals(command)))
                            {
                                day.Commands[command]++;
                            }
                            else
                            {
                                day.Commands.Add(command, 1);
                            }
                        }
                        else
                        {
                            month.Days.Add(new StatsDay(now.Day));
                            var _day = month.Days.First(d => d.Day == now.Day);
                            if (_day.Commands.Any(k => k.Key.Equals(command)))
                            {
                                _day.Commands[command]++;
                            }
                            else
                            {
                                _day.Commands.Add(command, 1);
                            }
                        }
                    }
                    else
                    {
                        year.Months.Add(new StatsMonth(now.Month, now.Day));
                        var day = year.Months.First(m => m.Month == now.Month)
                            .Days.First(d => d.Day == now.Day);
                        if (day.Commands.Any(k => k.Key.Equals(command)))
                        {
                            day.Commands[command]++;
                        }
                        else
                        {
                            day.Commands.Add(command, 1);
                        }
                    }
                }
                else
                {
                    user.Years.Add(new StatsYear(now.Year, now.Month, now.Day));
                    var day = user.Years.First(y => y.Year == now.Year)
                        .Months.First(m => m.Month == now.Month)
                        .Days.First(d => d.Day == now.Day);
                    if (day.Commands.Any(k => k.Key.Equals(command)))
                    {
                        day.Commands[command]++;
                    }
                    else
                    {
                        day.Commands.Add(command, 1);
                    }
                }
            }
            else
            {
                this.Users.Add(new StatsUser(uId, now.Year, now.Month, now.Day));
                var day = this.Users.First(u => u.Id == uId)
                    .Years.First(y => y.Year == now.Year)
                    .Months.First(m => m.Month == now.Month)
                    .Days.First(d => d.Day == now.Day);
                if (day.Commands.Any(k => k.Key.Equals(command)))
                {
                    day.Commands[command]++;
                }
                else
                {
                    day.Commands.Add(command, 1);
                }
            }
            return this;
        }
    }

    class AnalyticsCommandLoader
    {
        public List<Command> GetAnalyticsCommand()
        {
            List<Command> cmds = new List<Command>();

            Command analytics = new Command("analytics");
            analytics.RequiredPermission = Command.PermissionLevels.GlobalAdmin;
            analytics.Description = "Get a ton of analytics information from the server";
            analytics.ToExecute += async (client, msg, parameters) =>
            {
                var stats = new GuildMessageStats(msg.GetGuild().Id).DisposeLoader();
                var years = stats.Years;
                var months = years.SelectMany(y => y.Months);
                var days = months.SelectMany(m => m.Days);
                var users = days.SelectMany(d => d.Users);
                var commands = users.SelectMany(u => u.Commands);

                var mostActiveIdOverall = users.OrderByDescending(u => u.MessageCount).Take(3);
                string mostActiveUsersOverall = "";
                foreach (var id in mostActiveIdOverall)
                {
                    if (msg.GetGuild().Users.HasElement(u => u.Id == id.UserId, out var us))
                    {
                        mostActiveUsersOverall += $"    {us.GetDisplayName()} (`{us}`) " +
                        $"(`{id.MessageCount}` messages, `{id.Commands.Sum(c => c.Value)}` commands)\n";
                    }
                    else
                    {
                        mostActiveUsersOverall += $"    Unknown User (`{mostActiveIdOverall}`) " +
                        $"(`{id.MessageCount}` messages, `{id.Commands.Sum(c => c.Value)}` commands)\n";
                    }
                }

                var MostUsedCommandInfoOverall = commands.OrderByDescending(c => c.Value).Take(3);
                string MostUsedCommandsOverall = "";
                foreach (var cmd in MostUsedCommandInfoOverall)
                {
                    MostUsedCommandsOverall += $"    {cmd.Key} (`{cmd.Value}` uses)\n";
                }


                string info = $"Analytics for **{msg.GetGuild().Name}**\n\n" +
                $"All Messages Logged: `{users.Sum(u => u.MessageCount)}`\n" +
                $"All Commands Logged: `{commands.Sum(c => c.Value)}`\n" +
                $"Most Active Users Overall: \n{mostActiveUsersOverall}" +
                $"Most Used Commands Overall: \n{MostUsedCommandsOverall}";

                await msg.ReplyAsync(info);
            };

            //cmds.Add(analytics);

            return cmds;
        }
    }
}
