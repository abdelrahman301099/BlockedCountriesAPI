using System;

namespace assignment.Models
{
    public class LogEntry
    {
        public string IpAddress { get; set; }
        public string CountryCode { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime Timestamp { get; set; }
        public string RequestPath { get; set; }
        public string UserAgent { get; set; }
    }
} 