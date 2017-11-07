using System.Collections.Generic;

namespace GenericBot.Entities
{
    public class Giveaway
    {
        public string GiftName = "";
        public ulong GiverId = 0;
        public List<ulong> Hopefuls;
        public bool Open;

        public Giveaway()
        {
            Hopefuls = new List<ulong>();
            Open = true;
        }
    }
}
