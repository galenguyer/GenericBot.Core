using System;

namespace GenericBot.Entities
{
    public class UserBugReport
    {
        public string BugID;
        public ulong ReporterId;
        public string Report;
        public bool IsOpen;
        public string Repsonse = "";
        public DateTimeOffset ClosedAt;

        public UserBugReport()
        {
            IsOpen = true;
        }
    }
}
