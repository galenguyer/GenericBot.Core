using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SharperMark
{
    internal static class Extensions
    {
        public static void FindAndInsertNode(this List<MarkovNode> nodes, string word, string nextWord)
        {
            word = word.Trim();
            nextWord = nextWord.Trim();
            if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(nextWord))
                return;
            if (nodes.Any(n => n.Word == word))
                nodes.Find(n => n.Word == word).FollowingWords.Add(nextWord);
            else 
                nodes.Add(new MarkovNode { Word = word, FollowingWords = new List<string> { nextWord } });
        }
        public static T GetRandom<T>(this IEnumerable<T> input)
        {
            if (input.Count() == 0)
                return default(T);
            if (input.Count() == 1)
                return input.ElementAtOrDefault(0);

            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[4];
            crypto.GetNonZeroBytes(bytes);
            int rand = Math.Abs(BitConverter.ToInt32(bytes, 0)) % input.Count();
            return input.ElementAt(rand);
        }
    }
}
