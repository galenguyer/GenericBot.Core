using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericBot.Entities
{
    [BsonIgnoreExtraElements]
    public class Giveaway
    {
        public ulong OwnerId { get; set; }
        public string Description { get; set; }
        public string GiveawayId { get; set; }
        public List<ulong> EnteredUsers { get; set; }
        public bool IsActive { get; set; }

        public Giveaway()
        {

        }
    }
}
