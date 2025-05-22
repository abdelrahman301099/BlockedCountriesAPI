using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using assignment.Models;

namespace assignment.Services
{
    public class BlockedAttemptLogService
    {
        private readonly ConcurrentBag<BlockedAttemptLog> _logs;

        public BlockedAttemptLogService()
        {
            _logs = new ConcurrentBag<BlockedAttemptLog>();
        }

        public void LogAttempt(string ipAddress, string countryCode, bool isBlocked, string userAgent)
        {
            var log = new BlockedAttemptLog
            {
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                CountryCode = countryCode,
                IsBlocked = isBlocked,
                UserAgent = userAgent
            };

            _logs.Add(log);
        }

        public (IEnumerable<BlockedAttemptLog> Logs, int TotalCount) GetLogs(int page = 1, int pageSize = 10)
        {
            var query = _logs.AsQueryable();
            var totalCount = query.Count();
            
            var logs = query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (logs, totalCount);
        }

        public void ClearLogs()
        {
            while (!_logs.IsEmpty)
            {
                _logs.TryTake(out _);
            }
        }
    }
} 