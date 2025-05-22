using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using assignment.Models;

namespace assignment.Services
{
    public class LoggingService
    {
        private readonly ConcurrentDictionary<string, LogEntry> _logs;
        private const int MaxLogEntries = 10000; 

        public LoggingService()
        {
            _logs = new ConcurrentDictionary<string, LogEntry>();
        }

        public void AddLog(LogEntry log)
        {
            if (log == null) return;

            var key = $"{log.IpAddress}_{log.Timestamp:yyyyMMddHHmmss}";
            
            
            if (_logs.Count >= MaxLogEntries)
            {
                var oldestKey = _logs
                    .OrderBy(x => x.Value.Timestamp)
                    .First()
                    .Key;
                _logs.TryRemove(oldestKey, out _);
            }

            _logs.TryAdd(key, log);
        }

        public IEnumerable<LogEntry> GetLogs(
            int page = 1,
            int pageSize = 10,
            string searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _logs.Values.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToUpper();
                query = query.Where(l => 
                    l.IpAddress.ToUpper().Contains(searchTerm) ||
                    l.CountryCode.ToUpper().Contains(searchTerm) ||
                    l.UserAgent.ToUpper().Contains(searchTerm));
            }

            if (startDate.HasValue)
            {
                query = query.Where(l => l.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(l => l.Timestamp <= endDate.Value);
            }

            return query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public int GetTotalLogCount(
            string searchTerm = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _logs.Values.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToUpper();
                query = query.Where(l => 
                    l.IpAddress.ToUpper().Contains(searchTerm) ||
                    l.CountryCode.ToUpper().Contains(searchTerm) ||
                    l.UserAgent.ToUpper().Contains(searchTerm));
            }

            if (startDate.HasValue)
            {
                query = query.Where(l => l.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(l => l.Timestamp <= endDate.Value);
            }

            return query.Count();
        }

        public void ClearLogs()
        {
            _logs.Clear();
        }
    }
} 