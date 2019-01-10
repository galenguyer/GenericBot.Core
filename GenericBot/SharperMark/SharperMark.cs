using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SharperMark
{
    public class SimpleMarkov
    {
        internal int AverageSentenceLength = 0;
        internal List<MarkovNode> Nodes = new List<MarkovNode>();
        internal HashSet<string> StartingWords = new HashSet<string>();

        public SimpleMarkov()
        {
        }

        public void Train(string[] input)
        {
            foreach(string str in input)
            {
                var words = str.Split();
                if (words.Length < 2)
                    continue;

                AverageSentenceLength = (AverageSentenceLength + words.Length) / 2;
                StartingWords.Add(words[0]);
                for(int i = 0; i < words.Length-1; i++)
                {
                    Nodes.FindAndInsertNode(words[i], words[i + 1]);
                }
            }
        }

        public string GenerateWords(int count)
        {
            if (Nodes.Count == 0)
                throw new UntrainedException("Must train model before generating words");
            if (count < 1)
                throw new Exception("Word Count must be greater than 1");

            string chain = StartingWords.GetRandom<string>();
            string lastWord = chain;
            for (int generated = 1; generated < count; generated++)
            {
                var nextNode = Nodes.Find(n => n.Word == lastWord);
                if (nextNode == null)
                    nextNode = Nodes.GetRandom<MarkovNode>();
                string nextWord = nextNode.FollowingWords.GetRandom<string>();
                chain += " " + nextWord;
                lastWord = nextWord;
            }
            return chain;
        }
        public string[] GenerateSentences(int count)
        {
            string[] sentences = new string[count];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            for (int i = 0; i < count; i++)
            {
                byte[] bytes = new byte[4];
                crypto.GetNonZeroBytes(bytes);
                int rand = Math.Abs(BitConverter.ToInt32(bytes, 0)) % AverageSentenceLength*4;
                sentences[i] = GenerateWords(rand+2);
            }
            return sentences;
        }
        public string ToJson(Newtonsoft.Json.Formatting formatting = Formatting.None)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                this.AverageSentenceLength,
                this.Nodes,
                this.StartingWords
            }, formatting);
        }
        public SimpleMarkov FromJson(string inputJson)
        {
            var tmp = JsonConvert.DeserializeAnonymousType(inputJson, new { AverageSentenceLength, Nodes, StartingWords });
            this.Nodes = tmp.Nodes;
            this.AverageSentenceLength = tmp.AverageSentenceLength;
            this.StartingWords = tmp.StartingWords;
            return this;
        }
    }
}
