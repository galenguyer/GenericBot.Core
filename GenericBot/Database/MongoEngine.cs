using GenericBot.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericBot.Database
{
    public class MongoEngine : IDatabaseEngine
    {
        private MongoClient mongoClient;

        public MongoEngine()
        {
            if (!Core.GlobalConfig.DbConnectionString.StartsWith("mongodb"))
            {
                throw new Exception("Connection string is not of type mongo");
            }
            mongoClient = new MongoClient(Core.GlobalConfig.DbConnectionString);
        }

        /// <summary>
        /// Get the guild config if it's in the database, return a new config if not
        /// </summary>
        /// <param name="guildId">The GuildId to look up</param>
        /// <returns>A populated GuildConfig if found, otherwise a blank GuildConfig</returns>
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
        /// <param name="guildConfig">The GuildConfig to save</param>
        /// <returns>The saved GuildConfig</returns>
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

        /// <summary>
        /// Retrieve all Custom Commands from the database
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<CustomCommand> GetCustomCommands(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT CustomCommands FROM {guildId}");

            var _configDb = GetDatabaseFromGuildId(guildId);
            var _collection = _configDb.GetCollection<CustomCommand>("customCommands");
            var list = _collection.Find(new BsonDocument()).ToList();
            return list;
        }

        /// <summary>
        /// Delete a Custom Command from the database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guildId"></param>
        public void DeleteCustomCommand(string name, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] DELETE CustomCommand {name} FROM {guildId}");

            var _configDb = GetDatabaseFromGuildId(guildId);
            var _collection = _configDb.GetCollection<CustomCommand>("customCommands");

            if(_collection.Find(c => c.Name == name).Any())
                _collection.FindOneAndDelete(c => c.Name == name);
        }

        /// <summary>
        /// Look for a user in the guild's database, return a new user if not found
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public DatabaseUser GetUserFromGuild(ulong userId, ulong guildId, bool log = true)
        {
            if(log)
                Core.Logger.LogGenericMessage($"[Mongo] GOT User {userId} FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<DatabaseUser>("users");
            if (_collection.Find(u => u.Id == userId).Any())
                return _collection.Find(u => u.Id == userId).First();
            else return new DatabaseUser(userId);
        }

        /// <summary>
        /// Insert a user into a guild's database
        /// </summary>
        /// <param name="user"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public DatabaseUser SaveUserToGuild(DatabaseUser user, ulong guildId, bool log = true)
        {
            if (log)
                Core.Logger.LogGenericMessage($"[Mongo] SAVED User {user.Id} TO {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<DatabaseUser>("users");
            if (_collection.Find(u => u.Id == user.Id).Any())
                _collection.FindOneAndReplace(u => u.Id == user.Id, user);
            else _collection.InsertOne(user);
            return user;
        }

        /// <summary>
        /// Retrieve all users from a guild's database, useful for finding from nicknames
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<DatabaseUser> GetAllUsers(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT AllUsers FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<DatabaseUser>("users");

            return _collection.Find(new BsonDocument()).ToList();
        }

        /// <summary>
        /// Save a GenericBan to a guild
        /// </summary>
        /// <param name="ban"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Get all bans saved to a guild
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<GenericBan> GetBansFromGuild(ulong guildId, bool log = true)
        {
            if (log)
                Core.Logger.LogGenericMessage($"[Mongo] GOT AllBans FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<GenericBan>("bans");
            if (_collection.Find(new BsonDocument()).Any())
                return _collection.Find(new BsonDocument()).ToList();
            else return new List<GenericBan>();
        }

        /// <summary>
        /// Remove a ban from a guild after it's expired
        /// </summary>
        /// <param name="banId"></param>
        /// <param name="guildId"></param>
        public void RemoveBanFromGuild(ulong banId, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] DELETE Ban {banId} FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<GenericBan>("bans");
            if (_collection.Find(u => u.Id == banId).Any())
                _collection.DeleteOne(u => u.Id == banId);
        }

        /// <summary>
        /// Create a new Quote object and save it to the database
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Mark a quote as inactive in the database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
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
            catch (Exception ex)
            {
                Core.Logger.LogErrorMessage(ex, null);
                return false;
            }
        }
        
        /// <summary>
        /// Get all quotes from a guild to search over
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<Quote> GetAllQuotes(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT AllQuotes FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<Quote>("quotes");

            return _collection.Find(new BsonDocument("Active", true)).ToList();
        }

        /// <summary>
        /// Add an event to a guild's audit log
        /// </summary>
        /// <param name="command"></param>
        /// <param name="guildId"></param>
        public void AddToAuditLog(ParsedCommand command, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] SAVED AuditLog TO {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<AuditCommand>("auditlog");

            _collection.InsertOne(new AuditCommand(command));
        }

        /// <summary>
        /// Get all entries from a guild's audit log
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<AuditCommand> GetAuditLog(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT AuditLog FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<AuditCommand>("auditlog");

            return _collection.Find(new BsonDocument()).ToList();
        }

        public Giveaway CreateGiveaway(Giveaway giveaway, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] CREATE GIVEAWAY {giveaway.Id} FOR {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<Giveaway>("giveaways");

            while(_collection.Find(new BsonDocument()).ToList().HasElement(g => g.Id == giveaway.Id, out Giveaway output))
            {
                giveaway.Id += new Random().Next(0, 10).ToString();
            }

            _collection.InsertOne(giveaway);
            return giveaway;
        }

        public Giveaway UpdateOrCreateGiveaway(Giveaway giveaway, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] UPDATE GIVEAWAY {giveaway.Id} FOR {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<Giveaway>("giveaways");

            if (_collection.Find(new BsonDocument()).ToList().Any(g => g.Id == giveaway.Id))
            {
                _collection.FindOneAndReplace(g => g.Id == giveaway.Id, giveaway);
            }
            else _collection.InsertOne(giveaway);

            return giveaway;
        }

        public List<Giveaway> GetGiveaways(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT Giveaways FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<Giveaway>("giveaways");

            return _collection.Find(new BsonDocument()).ToList();
        }

        public void DeleteGiveaway(Giveaway giveaway, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] GOT Giveaways FROM {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<Giveaway>("giveaways");

            _collection.FindOneAndDelete(g => g.Id == giveaway.Id);
        }

        public void AddStatus(Status status)
        {
            var _db = mongoClient.GetDatabase("global");
            var _collection = _db.GetCollection<Status>("statusLog");
            _collection.InsertOne(status);
        }

        public ExceptionReport AddOrUpdateExceptionReport(ExceptionReport report)
        {
            var _db = mongoClient.GetDatabase("global");
            var _collection = _db.GetCollection<ExceptionReport>("exceptionReports");
            ExceptionReport foundReport;
            if (_collection.Find(new BsonDocument()).ToList()
                .HasElement(r => r.Message.Equals(report.Message) && r.StackTrace.Equals(report.StackTrace), out foundReport))
            {
                foundReport.Count++;
                _collection.FindOneAndReplace(r => r.Message.Equals(foundReport.Message) && r.StackTrace.Equals(foundReport.StackTrace), foundReport);
            }
            else
            {
                _collection.InsertOne(report);
                foundReport = report;
            }

            return foundReport;
        }

        public void AddVerification(ulong userId, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[Mongo] ADD VERIFICATION {userId} TO {guildId}");

            var _userDb = GetDatabaseFromGuildId(guildId);
            var _collection = _userDb.GetCollection<VerificationEvent>("verifications");

            var _event = new VerificationEvent(guildId, userId);
            _collection.InsertOne(_event);
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
