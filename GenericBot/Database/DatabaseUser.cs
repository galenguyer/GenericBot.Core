using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericBot.Database
{
    public class DatabaseUser
    {
        [BsonId]
        public ulong Id { get; set; }
        public List<string> Usernames { get; set; }
        public List<string> Nicknames { get; set; }
        public List<string> Warnings { get; set; }

        public DatabaseUser(ulong id)
        {
            this.Id = id;
            this.Usernames = new List<string>();
            this.Nicknames = new List<string>();
            this.Warnings = new List<string>();
        }
    }
}
