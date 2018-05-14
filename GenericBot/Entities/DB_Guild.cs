using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Timers;
using Discord.WebSocket;
using LiteDB;
using Newtonsoft.Json;

namespace GenericBot.Entities
{
    public class DBGuild
    {
        public ulong ID { get; set; }
        public List<DBUser> Users { get; set; }


        public DBGuild()
        {

        }

        public DBGuild(SocketGuild guild)
        {
            this.ID = guild.Id;
            this.Users = new List<DBUser>();
        }

        public DBGuild(ulong guildId)
        {
            this.ID = guildId;
            if (GenericBot.LoadedGuilds.ContainsKey(this.ID))
            {
                this.Users = GenericBot.LoadedGuilds[this.ID].Users;
            }
            else if (File.Exists($"files/guildDbs/{ID}.json"))
            {
                this.Users = JsonConvert.DeserializeObject<List<DBUser>>(AES.DecryptText(
                    File.ReadAllText($"files/guildDbs/{ID}.json"), GenericBot.DBPassword));
                GenericBot.LoadedGuilds.TryAdd(ID, this);
            }
            else
            {
                this.Users = new List<DBUser>();
            }
        }

        public async void Save()
        {
            GenericBot.LoadedGuilds[this.ID] = this;
            Directory.CreateDirectory("files");
            Directory.CreateDirectory("files/guildDbs");
            File.WriteAllText($"files/guildDbs/{ID}.json", AES.EncryptText(JsonConvert.SerializeObject(this.Users), GenericBot.DBPassword));
        }

        public DBGuild GetDBGuildFromId(ulong guildId)
        {
            var col = GenericBot.GlobalDatabase.GetCollection<DBGuild>("userDatabase");
            DBGuild tempdb;
            col.EnsureIndex(c => c.ID, true);
            if (col.Exists(c => c.ID.Equals(guildId)))
            {
                tempdb = col.FindOne(c => c.ID.Equals(guildId));
            }
            else
            {
                tempdb = new DBGuild(){ID = guildId, Users = new List<DBUser>()};
            }

            return tempdb;
        }

        public DBUser GetUser(ulong id)
        {
            if (Users.HasElement(u => u.ID.Equals(id), out var res))
            {
                return res;
            }
            else
            {
                Users.Add(new DBUser(){ID = id});
                return Users.First(u => u.ID == id);
            };
        }
    }
}
