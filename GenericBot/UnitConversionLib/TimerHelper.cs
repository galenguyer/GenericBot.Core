using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace UnitConversionLib
{
    public static class TimerHelper
    {
        public static string CreateNew(string st= null)
        {
            if (st == null)
                st = Guid.NewGuid().ToString();
            
            dic[st] = DateTime.Now;
            return st;
        }

        public static TimeSpan GetDuration(string st)
        {
            return DateTime.Now - dic[st];
        }
        private static Dictionary<string, DateTime> dic = new Dictionary<string, DateTime>();


    }
}
