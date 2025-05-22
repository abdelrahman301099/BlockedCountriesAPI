using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using assignment.Models;
using Microsoft.Extensions.Configuration;

namespace assignment.Services
{
    public class IpGeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public IpGeolocationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeolocationApi:ApiKey"];
            _baseUrl = configuration["GeolocationApi:BaseUrl"];
        }

        public bool IsValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return false;

            return IPAddress.TryParse(ipAddress, out _);
        }

        public async Task<IpLookupResult> GetIpDetailsAsync(string ipAddress)
        {
            if (!IsValidIpAddress(ipAddress))
                throw new ArgumentException("Invalid IP address format");

            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}?apiKey={_apiKey}&ip={ipAddress}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                
                return new IpLookupResult
                {
                    IpAddress = result.GetProperty("ip").GetString(),
                    CountryCode = result.GetProperty("country_code2").GetString(),
                    CountryName = result.GetProperty("country_name").GetString(),
                    City = result.GetProperty("city").GetString(),
                    
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get IP details: {ex.Message}");
            }
        }

        public async Task<string> GetCountryCodeFromIp(string ipAddress)
        {
            try
            {
                var details = await GetIpDetailsAsync(ipAddress);
                return details.CountryCode;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
} 