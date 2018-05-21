using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord.WebSocket;
using Hammock.Retries;
using Newtonsoft.Json;

namespace GenericBot.Entities
{
    public class DBGuild
    {
        public ulong ID { get; set; }
        public List<DBUser> Users { get; set; }
        public List<Quote> Quotes { get; set; }

        public DBGuild()
        {

        }

        public DBGuild(ulong guildId)
        {
            this.ID = guildId;
            if (GenericBot.LoadedGuilds.ContainsKey(this.ID))
            {
                this.Users = GenericBot.LoadedGuilds[this.ID].Users;
                this.Quotes = GenericBot.LoadedGuilds[this.ID].Quotes;
            }
            else if (File.Exists($"files/guildDbs/{ID}.json"))
            {
                var db = JsonConvert.DeserializeObject<DBGuild>(AES.DecryptText(
                    File.ReadAllText($"files/guildDbs/{ID}.json"), GenericBot.DBPassword));
                this.Users = db.Users;
                this.Quotes = db.Quotes;
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
            File.WriteAllText($"files/guildDbs/{ID}.json", AES.EncryptText(JsonConvert.SerializeObject(this), GenericBot.DBPassword));
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

        public Quote AddQuote(string content)
        {
            if (Quotes == null || Quotes.Count == 0)
            {
                Quotes = new List<Quote>();
            }
            var q = new Quote
            {
                Content = content,
                Id = Quotes.Count == 0 ? 1 : Quotes.Last().Id + 1,
                Active = true
            };
            Quotes.Add(q);

            this.Save();

            return Quotes.Last();
        }

        public Quote GetQuote(string identifer)
        {
            try
            {
                if (Quotes == null || Quotes.Count == 0)
                {
                    return new Quote {Content = "This server has no quotes", Id = -1};
                }

                if (string.IsNullOrEmpty(identifer))
                {
                    var ql = Quotes.Where(q => q.Active);
                    int max = ql.Count();
                    return ql.ElementAt(new Random().Next(0, max));
                }
                else
                {
                    if (int.TryParse(identifer.Trim(), out int id))
                    {
                        if (Quotes.Last(q => q.Active).Id >= id)
                        {
                            var quote = Quotes.Find(q => q.Id.Equals(id));
                            if (quote.Active)
                            {
                                return quote;
                            }
                            else
                            {
                                return new Quote {Content = "Quote deleted", Id = id};
                            }
                        }
                        else
                        {
                            return new Quote {Content = "Could not find quote", Id = -2};
                        }
                    }
                    else
                    {
                        var ql = Quotes.Where(q => q.Active)
                            .Where(q => q.Content.ToLower().Contains(identifer.ToLower()));
                        if (ql.Count() == 0)
                        {
                            return new Quote {Content = "Could not find quote", Id = -1};
                        }

                        int max = ql.Count();
                        return ql.ElementAt(new Random().Next(0, max));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
                return new Quote {Content = "Could not find quote", Id = -0};
            }
        }

        public bool RemoveQuote(int id)
        {
            if (id > Quotes.Last().Id)
            {
                return false;
            }

            var quote = Quotes.First(q => q.Id == id);
            quote.Active = false;
            return true;
        }
    }
}
