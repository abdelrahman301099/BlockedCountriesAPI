using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using assignment.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace assignment.Services
{
    public class BlockedCountriesService
    {
        private readonly ConcurrentDictionary<string, CountryInfo> _blockedCountries;
        private readonly ConcurrentDictionary<string, TemporalBlock> _temporalBlocks;
        private static readonly HashSet<string> _validCountryCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            "US", "GB", "CA", "AU", "DE", "FR", "IT", "ES", "JP", "CN", "IN", "BR", "RU", "ZA", "EG"
            // Add more valid country codes as needed
        };

        public BlockedCountriesService()
        {
            _blockedCountries = new ConcurrentDictionary<string, CountryInfo>();
            _temporalBlocks = new ConcurrentDictionary<string, TemporalBlock>();
        }

        public bool IsCountryBlocked(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
                return false;

            var upperCode = countryCode.ToUpper();
            return _blockedCountries.ContainsKey(upperCode) || 
                   (_temporalBlocks.TryGetValue(upperCode, out var block) && block.ExpiryTime > DateTime.UtcNow);
        }

        public bool BlockCountry(string countryCode, string countryName)
        {
            if (string.IsNullOrEmpty(countryCode))
                return false;

            var upperCode = countryCode.ToUpper();
            var countryInfo = new CountryInfo
            {
                Code = upperCode,
                Name = countryName
            };

            return _blockedCountries.TryAdd(upperCode, countryInfo);
        }

        public bool UnblockCountry(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
                return false;

            var upperCode = countryCode.ToUpper();
            return _blockedCountries.TryRemove(upperCode, out _);
        }

        public (IEnumerable<CountryInfo> Countries, int TotalCount) GetBlockedCountries(
            int page = 1, 
            int pageSize = 10, 
            string searchTerm = null)
        {
            var query = _blockedCountries.Values.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToUpper();
                query = query.Where(c => 
                    c.Code.Contains(searchTerm) || 
                    c.Name.ToUpper().Contains(searchTerm));
            }

            var totalCount = query.Count();
            var countries = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (countries, totalCount);
        }

        public bool AddTemporalBlock(string countryCode, int durationMinutes)
        {
            if (string.IsNullOrEmpty(countryCode) || !IsValidCountryCode(countryCode))
                return false;

            if (durationMinutes < 1 || durationMinutes > 1440)
                return false;

            var upperCode = countryCode.ToUpper();
            var temporalBlock = new TemporalBlock
            {
                CountryCode = upperCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(durationMinutes)
            };

            return _temporalBlocks.TryAdd(upperCode, temporalBlock);
        }

        public bool IsTemporarilyBlocked(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
                return false;

            var upperCode = countryCode.ToUpper();
            return _temporalBlocks.TryGetValue(upperCode, out var block) && 
                   block.ExpiryTime > DateTime.UtcNow;
        }

        public void CleanupExpiredTemporalBlocks()
        {
            var expiredBlocks = _temporalBlocks
                .Where(kvp => kvp.Value.ExpiryTime <= DateTime.UtcNow)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var countryCode in expiredBlocks)
            {
                _temporalBlocks.TryRemove(countryCode, out _);
            }
        }

        private bool IsValidCountryCode(string countryCode)
        {
            return !string.IsNullOrEmpty(countryCode) && 
                   countryCode.Length == 2 && 
                   _validCountryCodes.Contains(countryCode.ToUpper());
        }
    }
} 