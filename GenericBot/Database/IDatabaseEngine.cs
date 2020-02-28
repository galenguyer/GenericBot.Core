using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericBot.Database
{
    interface IDatabaseEngine
    {
        /// <summary>
        /// Get the guild config if it's in the database, return a new config if not
        /// </summary>
        /// <param name="guildId">The GuildId to look up</param>
        /// <returns>A populated GuildConfig if found, otherwise a blank GuildConfig</returns>
        public GuildConfig GetGuildConfig(ulong guildId);

        /// <summary>
        /// Update the config in the database if it exists, add it if not
        /// </summary>
        /// <param name="guildConfig">The GuildConfig to save</param>
        /// <returns>The saved GuildConfig</returns>
        public GuildConfig SaveGuildConfig(GuildConfig guildConfig);

        /// <summary>
        /// Add a command to the database, overwrite if exists already.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public CustomCommand SaveCustomCommand(CustomCommand command, ulong guildId);

        /// <summary>
        /// Retrieve all Custom Commands from the database
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<CustomCommand> GetCustomCommands(ulong guildId);

        /// <summary>
        /// Delete a Custom Command from the database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guildId"></param>
        public void DeleteCustomCommand(string name, ulong guildId);

        /// <summary>
        /// Look for a user in the guild's database, return a new user if not found
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public DatabaseUser GetUserFromGuild(ulong userId, ulong guildId, bool log = true);

        /// <summary>
        /// Insert a user into a guild's database
        /// </summary>
        /// <param name="user"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public DatabaseUser SaveUserToGuild(DatabaseUser user, ulong guildId, bool log = true);

        /// <summary>
        /// Retrieve all users from a guild's database, useful for finding from nicknames
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<DatabaseUser> GetAllUsers(ulong guildId);

        /// <summary>
        /// Save a GenericBan to a guild
        /// </summary>
        /// <param name="ban"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public GenericBan SaveBanToGuild(GenericBan ban, ulong guildId);

        /// <summary>
        /// Get all bans saved to a guild
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<GenericBan> GetBansFromGuild(ulong guildId, bool log = true);

        /// <summary>
        /// Remove a ban from a guild after it's expired
        /// </summary>
        /// <param name="banId"></param>
        /// <param name="guildId"></param>
        public void RemoveBanFromGuild(ulong banId, ulong guildId);

        /// <summary>
        /// Create a new Quote object and save it to the database
        /// </summary>
        /// <param name="quote"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public Quote AddQuote(string quote, ulong guildId);

        /// <summary>
        /// Mark a quote as inactive in the database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public bool RemoveQuote(int id, ulong guildId);

        /// <summary>
        /// Get all quotes from a guild to search over
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<Quote> GetAllQuotes(ulong guildId);

        /// <summary>
        /// Add an event to a guild's audit log
        /// </summary>
        /// <param name="command"></param>
        /// <param name="guildId"></param>
        public void AddToAuditLog(ParsedCommand command, ulong guildId);

        /// <summary>
        /// Get all entries from a guild's audit log
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public List<AuditCommand> GetAuditLog(ulong guildId);

        public Giveaway CreateGiveaway(Giveaway giveaway, ulong guildId);

        public Giveaway UpdateOrCreateGiveaway(Giveaway giveaway, ulong guildId);

        public List<Giveaway> GetGiveaways(ulong guildId);

        public void DeleteGiveaway(Giveaway giveaway, ulong guildId);

        public void AddStatus(Status status);

        public ExceptionReport AddOrUpdateExceptionReport(ExceptionReport report);

        public List<string> GetGuildIdsFromDb();

        private IMongoDatabase GetDatabaseFromGuildId(ulong GuildId);
    }
}
