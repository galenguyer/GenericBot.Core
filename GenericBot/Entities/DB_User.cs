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
        public string VerifiedInfo { get; set; }

        public DBUser()
        {

        }
        public DBUser(SocketGuildUser user)
        {
            ID = user.Id;
            Nicknames = new List<string>();
            Usernames = new List<string>{user.Username};
            Warnings = new List<string>();

            if(!string.IsNullOrEmpty(user.Nickname))
                Nicknames.Add(user.Nickname);
            CreatedAt = user.CreatedAt;
        }

        public void AddUsername(string username)
        {
            if (!Usernames.Contains(username))
            {
                Usernames.Add(username);
            }
        }

        public void AddNickname(SocketGuildUser user)
        {
            if (!string.IsNullOrEmpty(user.Nickname) && !Nicknames.Contains(user.Nickname))
            {
                Nicknames.Add(user.Nickname);
            }
        }

        public void AddWarning(string warning)
        {
            if(Warnings == null) Warnings = new List<string>();
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
