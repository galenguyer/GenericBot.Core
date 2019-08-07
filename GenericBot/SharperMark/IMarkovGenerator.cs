using System;
using System.Collections.Generic;
using System.Text;

namespace SharperMark
{
    public interface IMarkovGenerator
    {
        void Train(string[] input);

        string GenerateWords(int count);
    }
}
