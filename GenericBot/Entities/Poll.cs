using System;
using System.Collections.Generic;

namespace GenericBot.Entities
{
    public class Poll
    {
        public string Text;
        public bool MultipleChoice;
        public ulong Creator;
        public ulong MessageId;
        public List<PollOption> Options = new List<PollOption>();

        public bool Vote(ulong userId, int option)
        {
            if (option < 0 || option >= Options.Count) return false;
            
            var alreadyVotedFor = GetVoted(userId);
            if (!MultipleChoice)
            {
                foreach (var opt in alreadyVotedFor) opt.Voters.Remove(userId);
            }

            Options[option].Voters.Add(userId);
            return true;
        }
        
        public bool HasVoted(ulong userId)
        {
            return GetVoted(userId) != null;
        }

        public List<PollOption> GetVoted(ulong userId)
        {
            List<PollOption> options = new List<PollOption>();
            foreach (var opt in Options)
            {
                if (opt.Voters.Contains(userId)) options.Add(opt);
            }
            return options;
        }
    }

    public class PollOption
    {
        public string Text;
        public HashSet<ulong> Voters = new HashSet<ulong>();
    }
}