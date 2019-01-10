using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharperMark
{
    public class LookbackMarkov
    {
        private int AverageSentenceLength = 0;
        private List<MarkovNode> Nodes = new List<MarkovNode>();
        private HashSet<string> StartingWords = new HashSet<string>();

        public LookbackMarkov()
        {
        }

        public void Train(string[] input)
        {
            foreach (string str in input)
            {
                var words = str.Split();
                if (words.Length < 3)
                    continue;

                AverageSentenceLength = (AverageSentenceLength + words.Length) / 2;
                StartingWords.Add($"{words[0]} {words[1]}");
                for (int i = 0; i < words.Length - 2; i++)
                {
                    Nodes.FindAndInsertNode($"{words[i]} {words[i+1]}", words[i + 2]);
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

                int spaceCount = chain.Count(f => f == ' ');
                lastWord = $"{chain.Split()[spaceCount - 1]} {chain.Split()[spaceCount]}";
            }
            return chain;
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
        public LookbackMarkov FromJson(string inputJson)
        {
            var tmp = JsonConvert.DeserializeAnonymousType(inputJson, new { AverageSentenceLength, Nodes, StartingWords });
            this.Nodes = tmp.Nodes;
            this.AverageSentenceLength = tmp.AverageSentenceLength;
            this.StartingWords = tmp.StartingWords;
            return this;
        }

    }
}
