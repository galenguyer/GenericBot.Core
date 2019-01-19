using System;
using System.Collections.Generic;
using Discord.WebSocket;

namespace GenericBot.Entities
{
    public class DBUser
    {
        public ulong ID { get; set; }
        public List<string> Nicknames { get; set; }
        public List<string> Usernames { get; set; }
        public List<string> Warnings { get; set; }
        public List<ulong> SavedRoles { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public decimal PointsCount { get; set; }
        public DateTimeOffset LastThanks { get; set; }
        public DBUser()
        {

        }
        public DBUser(SocketGuildUser user)
        {
            ID = user.Id;
            Nicknames = new List<string>();
            Usernames = new List<string> { user.Username };
            Warnings = new List<string>();
            PointsCount = 0;
            LastThanks = DateTimeOffset.FromUnixTimeSeconds(0);

            if (!string.IsNullOrEmpty(user.Nickname))
                Nicknames.Add(user.Nickname);
            CreatedAt = user.CreatedAt;
        }

        public void AddUsername(string username)
        {
            if (Usernames == null) Usernames = new List<string>();
            if (!Usernames.Contains(username))
            {
                Usernames.Add(username);
            }
        }
        public void AddNickname(string username)
        {
            if (Nicknames == null) Nicknames = new List<string>();
            if (!Nicknames.Contains(username))
            {
                Nicknames.Add(username);
            }
        }

        public void AddNickname(SocketGuildUser user)
        {
            if (Nicknames == null) Nicknames = new List<string>();
            if (user.Nickname != null && !string.IsNullOrEmpty(user.Nickname) && !Nicknames.Contains(user.Nickname))
            {
                Nicknames.Add(user.Nickname);
            }
        }

        public void AddWarning(string warning)
        {
            if (Warnings == null) Warnings = new List<string>();
            Warnings.Add(warning);
        }

        public bool RemoveWarning(bool allWarnings = false)
        {
            if (Warnings.Empty())
            {
                return false;
            }
            if (!allWarnings)
            {
                Warnings.RemoveAt(Warnings.Count - 1);
            }
            else
            {
                Warnings.RemoveRange(0, Warnings.Count);
            }
            return true;
        }
    }
}
