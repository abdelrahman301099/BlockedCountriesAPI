using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using assignment.Models;
using System.Threading;

namespace assignment.Services
{
    public class GeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly ILogger<GeolocationService> _logger;
        private readonly SemaphoreSlim _rateLimiter;
        private const int MaxRequestsPerMinute = 30; 

        public GeolocationService( HttpClient httpClient, IConfiguration configuration,ILogger<GeolocationService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeolocationApi:ApiKey"];
            _baseUrl = configuration["GeolocationApi:BaseUrl"];
            _logger = logger;
            _rateLimiter = new SemaphoreSlim(1, 1);


            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BlockedCountriesAPI/1.0");
        }

        public async Task<IpLookupResult> GetIpDetailsAsync(string ipAddress)
        {
            if (!IsValidIpAddress(ipAddress))
                throw new ArgumentException("Invalid IP address format", nameof(ipAddress));

            try
            {
                await _rateLimiter.WaitAsync();
                var url = $"{_baseUrl}?apiKey={_apiKey}&ip={ipAddress}";
                Console.WriteLine(url);
                using var response = await _httpClient.GetAsync(url);
                
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("Rate limit exceeded for IP lookup");
                    throw new Exception("API rate limit exceeded. Please try again later.");
                }

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(content);

                return new IpLookupResult
                {
                    IpAddress = ipAddress,
                    CountryCode = result.country_code2?.ToString(),
                    CountryName = result.country_name?.ToString(),
                    Isp = result.isp?.ToString(),
                    City = result.city?.ToString(),
                    Region = result.state_prov?.ToString(),
                    Latitude = result.latitude?.ToString(),
                    Longitude = result.longitude?.ToString(),
                    TimeZone = result.time_zone?.name?.ToString()
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for IP {IpAddress}", ipAddress);
                throw new Exception($"Failed to get IP details: {ex.Message}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse response for IP {IpAddress}", ipAddress);
                throw new Exception("Failed to parse API response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error for IP {IpAddress}", ipAddress);
                throw;
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        public bool IsValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return false;

            var parts = ipAddress.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out int value) || value < 0 || value > 255)
                    return false;
            }

            return true;
        }
    }
} 