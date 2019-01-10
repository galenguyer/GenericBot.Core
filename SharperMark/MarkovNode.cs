using System;
using System.Collections.Generic;
using System.Text;

namespace SharperMark
{
    internal class MarkovNode
    {
        public string Word;
        public List<string> FollowingWords;
    }
}
