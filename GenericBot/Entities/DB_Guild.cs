using System.Collections.Generic;

namespace GenericBot.Entities
{
    public class DBGuild
    {
        public ulong ID { get; set; }
        public List<DBUser> Users { get; set; }

        public DBGuild()
        {

        }
        public DBGuild(ulong Id)
        {
            ID = Id;
            Users = new List<DBUser>();
        }
    }
}
