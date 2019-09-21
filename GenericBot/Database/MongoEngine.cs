using GenericBot.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericBot.Database
{
    public class MongoEngine
    {
        private MongoClient mongoClient;

        public MongoEngine()
        {
            mongoClient = new MongoClient(Core.GlobalConfig.DbConnectionString);
        }

        public GuildConfig GetGuildConfig(ulong GuildId)
        {
            var _configDb = mongoClient.GetDatabase(GuildId.ToString());
            var _collection = _configDb.GetCollection<GuildConfig>("config");

            if (_collection.Find(c => c.Id == GuildId).Any())
                return _collection.Find(c => c.Id == GuildId).First();
            else
                return new GuildConfig(GuildId);
        }
    }
}
