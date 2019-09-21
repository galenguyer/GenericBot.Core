using GenericBot.Entities;
using System;

namespace GenericBot
{
    class GenericBot
    {
        private static GlobalConfiguration globalConfig;
        static void Main(string[] args)
        {
            globalConfig = new GlobalConfiguration().Load();

        }
    }
}
