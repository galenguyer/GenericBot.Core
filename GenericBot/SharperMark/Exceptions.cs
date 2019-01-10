using System;
using System.Collections.Generic;
using System.Text;

namespace SharperMark
{
    internal class UntrainedException : Exception
    {
        public UntrainedException() : base() { }
        public UntrainedException(string message) : base(message) { }

    }
}
