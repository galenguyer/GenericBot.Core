﻿using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericBot.Entities
{
    public class ExceptionReport
    {
        public enum ReportAction
        {
            None, 
            OpenNewIssue
        }

        [BsonId]
        public int Id { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public int Count { get; set; }
        public bool Reported { get; set; }
        public ExceptionReport()
        {

        }

        public ExceptionReport(Exception ex)
        {
            this.Message = ex.Message;
            this.StackTrace = ex.StackTrace ?? string.Empty;

            this.Id = this.Message.GetHashCode() + this.StackTrace.GetHashCode();

            this.Count = 1;
            this.Reported = false;
        }
    }
}
