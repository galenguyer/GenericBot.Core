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
            throw new NotImplementedException();
        }

        public Quote AddQuote(string quote, ulong guildId)
        {
            throw new NotImplementedException();
        }

        public void AddStatus(Status status)
        {
            throw new NotImplementedException();
        }

        public void AddToAuditLog(ParsedCommand command, ulong guildId)
        {
            throw new NotImplementedException();
        }

        public Giveaway CreateGiveaway(Giveaway giveaway, ulong guildId)
        {
            throw new NotImplementedException();
        }

        public void DeleteCustomCommand(string name, ulong guildId)
        {
            throw new NotImplementedException();
        }

        public void DeleteGiveaway(Giveaway giveaway, ulong guildId)
        {
            throw new NotImplementedException();
        }

        public List<Quote> GetAllQuotes(ulong guildId)
        {
            throw new NotImplementedException();
        }

        public List<DatabaseUser> GetAllUsers(ulong guildId)
        {
            throw new NotImplementedException();
        }

        public List<AuditCommand> GetAuditLog(ulong guildId)
        {
            throw new NotImplementedException();
        }

        public List<GenericBan> GetBansFromGuild(ulong guildId, bool log = true)
        {
            throw new NotImplementedException();
        }

        public List<CustomCommand> GetCustomCommands(ulong guildId)
        {
            throw new NotImplementedException();
        }

        public List<Giveaway> GetGiveaways(ulong guildId)
        {
            throw new NotImplementedException();
        }

        public GuildConfig GetGuildConfig(ulong guildId)
        {

        }

        public List<string> GetGuildIdsFromDb()
        {
            throw new NotImplementedException();
        }

        public DatabaseUser GetUserFromGuild(ulong userId, ulong guildId, bool log = true)
        {
            throw new NotImplementedException();
        }

        public void RemoveBanFromGuild(ulong banId, ulong guildId)
        {
            throw new NotImplementedException();
        }

        public bool RemoveQuote(int id, ulong guildId)
        {
            throw new NotImplementedException();
        }

        public GenericBan SaveBanToGuild(GenericBan ban, ulong guildId)
        {
            throw new NotImplementedException();
        }

        public CustomCommand SaveCustomCommand(CustomCommand command, ulong guildId)
        {
            throw new NotImplementedException();
        }

        public GuildConfig SaveGuildConfig(GuildConfig guildConfig)
        {
            throw new NotImplementedException();
        }

        public DatabaseUser SaveUserToGuild(DatabaseUser user, ulong guildId, bool log = true)
        {
            throw new NotImplementedException();
        }

        public Giveaway UpdateOrCreateGiveaway(Giveaway giveaway, ulong guildId)
        {
            throw new NotImplementedException();
        }
    }
}
