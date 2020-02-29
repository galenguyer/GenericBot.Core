using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericBot.Entities;
using LiteDB;

namespace GenericBot.Database
{
    public class LiteDbEngine : IDatabaseEngine
    {
        private LiteDatabase liteDatabase;

        public LiteDbEngine()
        {
            liteDatabase = new LiteDatabase(Core.GlobalConfig.DbConnectionString);
        }

        public ExceptionReport AddOrUpdateExceptionReport(ExceptionReport report)
        {
            var _exceptionDb = liteDatabase.GetCollection<ExceptionReport>($"exceptions");
            ExceptionReport foundReport;
            if (_exceptionDb.FindAll().ToList()
                .HasElement(r => r.Message.Equals(report.Message) && r.StackTrace.Equals(report.StackTrace), out foundReport))
            {
                foundReport.Count++;
                _exceptionDb.Upsert(foundReport);
            }
            else
            {
                _exceptionDb.Insert(report);
                foundReport = report;
            }
            return foundReport;
        }

        public Quote AddQuote(string quote, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] SAVED Quote TO {guildId}");
            var _quoteDb = liteDatabase.GetCollection<Quote>($"{guildId}-quotes");
            var q = new Quote
            {
                Content = quote,
                Id = _quoteDb.Count() == 0 ? 1 : (int)_quoteDb.Count() + 1,
                Active = true
            };
            _quoteDb.Insert(q);
            return q;
        }

        public List<Quote> GetAllQuotes(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] SAVED Quote TO {guildId}");
            var _quoteDb = liteDatabase.GetCollection<Quote>($"{guildId}-quotes");
            return _quoteDb.FindAll().ToList();
        }

        public bool RemoveQuote(int id, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] DELETE Quote {id} FROM {guildId}");
            var _quoteDb = liteDatabase.GetCollection<Quote>($"{guildId}-quotes");
            var _quote = _quoteDb.FindOne(q => q.Id == id);
            _quote.Active = false;
            try
            {
                _quoteDb.Upsert(_quote);
                return true;
            }
            catch (Exception ex)
            {
                Core.Logger.LogErrorMessage(ex, null);
                return false;
            }
        }

        public void AddStatus(Status status)
        {
            var _db = liteDatabase.GetCollection<Status>($"statusLog");
            _db.Insert(status);
        }

        public void AddToAuditLog(ParsedCommand command, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] SAVED AuditLog TO {guildId}");
            var _db = liteDatabase.GetCollection<AuditCommand>($"{guildId}-auditLog");
            _db.Insert(new AuditCommand(command));
        }

        public List<AuditCommand> GetAuditLog(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] GOT AuditLog FROM {guildId}");
            var _db = liteDatabase.GetCollection<AuditCommand>($"{guildId}-auditLog");
            return _db.FindAll().ToList();
        }

        public CustomCommand SaveCustomCommand(CustomCommand command, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] SAVED CustomComand {command.Name} TO {guildId}");
            var _db = liteDatabase.GetCollection<CustomCommand>($"{guildId}-commands");
            _db.Upsert(command);
            return command;
        }

        public List<CustomCommand> GetCustomCommands(ulong guildId)
        {
            var _db = liteDatabase.GetCollection<CustomCommand>($"{guildId}-commands");
            return _db.FindAll().ToList();
        }

        public void DeleteCustomCommand(string name, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] DELETE CustomCommand {name} FROM {guildId}");
            var _db = liteDatabase.GetCollection<CustomCommand>($"{guildId}-commands");
            _db.DeleteMany(c => c.Name == name);
        }

        public List<Giveaway> GetGiveaways(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDB] GOT giveaways FROM {guildId}");
            var _db = liteDatabase.GetCollection<Giveaway>($"{guildId}-giveaways");

            return _db.FindAll().ToList();
        }

        public Giveaway UpdateOrCreateGiveaway(Giveaway giveaway, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDB] UPDATE giveaways FROM {guildId}");
            var _giveawayDb = liteDatabase.GetCollection<Giveaway>($"{guildId}-giveaways");
            _giveawayDb.Upsert(giveaway);
            return giveaway;
        }

        public Giveaway CreateGiveaway(Giveaway giveaway, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] CREATE GIVEAWAY {giveaway.Id} FOR {guildId}");
            var _giveawayDb = liteDatabase.GetCollection<Giveaway>($"{guildId}-giveaways");
            while (_giveawayDb.Exists(g => g.Id == giveaway.Id))
            {
                giveaway.Id += new Random().Next(0, 10).ToString();
            }
            _giveawayDb.Insert(giveaway);
            return giveaway;
        }

        public void DeleteGiveaway(Giveaway giveaway, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDB] DELETE giveaway {giveaway.Id} FROM {guildId}");
            var _giveawayDb = liteDatabase.GetCollection<Giveaway>($"{guildId}-giveaways");
            _giveawayDb.DeleteMany(g => g.Id == giveaway.Id);
        }

        public DatabaseUser GetUserFromGuild(ulong userId, ulong guildId, bool log = true)
        {
            if (log)
                Core.Logger.LogGenericMessage($"[LiteDb] GOT User {userId} FROM {guildId}");
            var _db = liteDatabase.GetCollection<DatabaseUser>($"{guildId}-users");
            if (_db.Exists(u => u.Id == userId))
                return _db.Find(u => u.Id == userId).First();
            else return new DatabaseUser(userId);
        }

        public DatabaseUser SaveUserToGuild(DatabaseUser user, ulong guildId, bool log = true)
        {
            if (log)
                Core.Logger.LogGenericMessage($"[LiteDb] SAVED User {user.Id} TO {guildId}");
            var _db = liteDatabase.GetCollection<DatabaseUser>($"{guildId}-users");
            _db.Upsert(user);
            return user;
        }

        public List<DatabaseUser> GetAllUsers(ulong guildId)
        {
            var _db = liteDatabase.GetCollection<DatabaseUser>($"{guildId}-users");
            return _db.FindAll().ToList();
        }

        public void RemoveBanFromGuild(ulong banId, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDB] DELETE Ban {banId} FROM {guildId}");

            var _db = liteDatabase.GetCollection<GenericBan>($"{guildId}-bans");
            if (_db.Exists(b => b.Id == banId))
                _db.DeleteMany(b => b.Id == banId);
        }

        public List<GenericBan> GetBansFromGuild(ulong guildId, bool log = true)
        {
            Core.Logger.LogGenericMessage($"[LiteDB] GET bans FROM {guildId}");
            var _db = liteDatabase.GetCollection<GenericBan>($"{guildId}-bans");
            return _db.FindAll().ToList();
        }

        public GenericBan SaveBanToGuild(GenericBan ban, ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] SAVED Ban {ban.Id} TO {guildId}");
            var _db = liteDatabase.GetCollection<GenericBan>($"{guildId}-bans");
            _db.Upsert(ban);
            return ban;
        }

        public GuildConfig SaveGuildConfig(GuildConfig guildConfig)
        {
            Core.Logger.LogGenericMessage($"[LiteDb] SAVED GuildConfig TO {guildConfig.Id}");

            var _db = liteDatabase.GetCollection<GuildConfig>($"{guildConfig.Id}-config");
            _db.Upsert(guildConfig);
            return guildConfig;
        }

        public GuildConfig GetGuildConfig(ulong guildId)
        {
            Core.Logger.LogGenericMessage($"[LiteDB] GOT GuildConfig FROM {guildId}");
            var _configDb = liteDatabase.GetCollection<GuildConfig>($"{guildId}-config");
            if (_configDb.Find(c => c.Id == guildId).Any())
                return _configDb.Find(c => c.Id == guildId).First();
            else
                return new GuildConfig(guildId);
        }

        public List<string> GetGuildIdsFromDb()
        {
            var collectionNames = liteDatabase.GetCollectionNames().Select(s => s.Split('-')[1]).Distinct();
            return collectionNames.ToList();
        }
    }
}
