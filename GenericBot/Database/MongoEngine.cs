using GenericBot.Entities;
using MongoDB.Bson;
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

        /// <summary>
        /// Get the guild config if it's in the database, return a new config if not
        /// </summary>
        /// <param name="GuildId"></param>
        /// <returns></returns>
        public GuildConfig GetGuildConfig(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT GuildConfig FROM {guildId}");
            var _configDb = GetDatabaseFromGuildId(guildId);
            var _collection = _configDb.GetCollection<GuildConfig>("config");

            if (_collection.Find(c => c.Id == guildId).Any())
                return _collection.Find(c => c.Id == guildId).First();
            else
                return new GuildConfig(guildId);
        }
        /// <summary>
        /// Update the config in the database if it exists, add it if not
        /// </summary>
        /// <param name="guildConfig"></param>
        /// <returns></returns>
        public GuildConfig SaveGuildConfig(GuildConfig guildConfig)
        {
            Core.Logger.LogGenericMessage($"[Mongo] SAVED GuildConfig TO {guildConfig.Id}");

            var _configDb = GetDatabaseFromGuildId(guildConfig.Id);
            var _collection = _configDb.GetCollection<GuildConfig>("config");
            if (_collection.Find(c => c.Id == guildConfig.Id).Any())
                _collection.ReplaceOne(c => c.Id == guildConfig.Id, guildConfig);
            else _collection.InsertOne(guildConfig);

            return guildConfig;
        }
        /// <summary>
        /// Add a command to the database, overwrite if exists already.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public CustomCommand SaveCustomCommand(CustomCommand command, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] SAVED CustomComand {command.Name} TO {guildId}");

            var _configDb = GetDatabaseFromGuildId(guildId);
            var _collection = _configDb.GetCollection<CustomCommand>("customCommands");
            if (_collection.Find(c => c.Name == command.Name).Any())
                _collection.ReplaceOne(c => c.Name == command.Name, command);
            else _collection.InsertOne(command);
            return command;
        }
        public List<CustomCommand> GetCustomCommands(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT CustomCommands FROM {guildId}");

            var _configDb = GetDatabaseFromGuildId(guildId);
            var _collection = _configDb.GetCollection<CustomCommand>("customCommands");
            var list = _collection.Find(new BsonDocument()).ToList();
            return list;
        }
        public DatabaseUser GetUserFromGuild(ulong userId, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT User {userId} FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<DatabaseUser>("users");
            if (_collection.Find(u => u.Id == userId).Any())
                return _collection.Find(u => u.Id == userId).First();
            else return new DatabaseUser(userId);
        }
        public DatabaseUser SaveUserToGuild(DatabaseUser user, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] SAVED User {user.Id} TO {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<DatabaseUser>("users");
            if (_collection.Find(u => u.Id == user.Id).Any())
                _collection.FindOneAndReplace(u => u.Id == user.Id, user);
            else _collection.InsertOne(user);
            return user;
        }
        public List<DatabaseUser> GetAllUsers(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT AllUsers FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<DatabaseUser>("users");

            return _collection.Find(new BsonDocument()).ToList();
        }
        public GenericBan SaveBanToGuild(GenericBan ban, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] SAVED Ban {ban.Id} TO {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<GenericBan>("bans");
            if (_collection.Find(u => u.Id == ban.Id).Any())
                _collection.FindOneAndReplace(u => u.Id == ban.Id, ban);
            else _collection.InsertOne(ban);
            return ban;
        }
        
        public List<GenericBan> GetBansFromGuild(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT AllBans FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<GenericBan>("bans");
            if (_collection.Find(new BsonDocument()).Any())
                return _collection.Find(new BsonDocument()).ToList();
            else return new List<GenericBan>();
        }

        public void RemoveBanFromGuild(ulong banId, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] DELETE Ban {banId} FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<GenericBan>("bans");
            if (_collection.Find(u => u.Id == banId).Any())
                _collection.DeleteOne(u => u.Id == banId);
        }

        public Quote AddQuote(string quote, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] SAVED Quote TO {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<Quote>("quotes");

            var q = new Quote
            {
                Content = quote,
                Id = _collection.CountDocuments(new BsonDocument()) == 0 ? 1 : (int)_collection.CountDocuments(new BsonDocument()) + 1,
                Active = true
            };
            _collection.InsertOne(q);

            return q;
        }

        public bool RemoveQuote(int id, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] DELETE Quote {id} FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<Quote>("quotes");

            try
            {
                _collection.UpdateOne(new BsonDocument("_id", id), Builders<Quote>.Update.Set("Active", false));
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public List<Quote> GetAllQuotes(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT AllQuotes FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<Quote>("quotes");

            return _collection.Find(new BsonDocument("Active", true)).ToList();
        }

        public void AddToAuditLog(ParsedCommand command, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] SAVED AuditLog TO {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<AuditCommand>("auditlog");

            _collection.InsertOne(new AuditCommand(command));
        }
        public List<AuditCommand> GetAuditLog(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT AuditLog FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<AuditCommand>("auditlog");

            return _collection.Find(new BsonDocument()).ToList();
        }

        public List<string> GetGuildIdsFromDb()
        {
            return mongoClient.ListDatabaseNames().ToList();
        }
        private IMongoDatabase GetDatabaseFromGuildId(ulong GuildId)
        {
            return mongoClient.GetDatabase(GuildId.ToString());
        }
    }
}
