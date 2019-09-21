using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericBot.Database
{
    class DatabaseGuild
    {
        [BsonId]
        public ulong Id { get; set; }
        public List<DatabaseUser> Users { get; set; }

        public DatabaseGuild(ulong id)
        {
            this.Id = id;
            this.Users = new List<DatabaseUser>();
        }
    }
}
