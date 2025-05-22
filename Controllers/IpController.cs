using Microsoft.AspNetCore.Mvc;
using assignment.Services;
using assignment.Models;
using System.Net;

namespace assignment.Controllers
{
    [ApiController]
    [Route("api/ip")]
    public class IpController : ControllerBase
    {
        private readonly GeolocationService _geolocationService;
        private readonly BlockedCountriesService _blockedCountriesService;
        private readonly LoggingService _loggingService;

        public IpController(
            GeolocationService geolocationService,
            BlockedCountriesService blockedCountriesService,
            LoggingService loggingService)
        {
            _geolocationService = geolocationService;
            _blockedCountriesService = blockedCountriesService;
            _loggingService = loggingService;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> LookupIp([FromQuery] string ipAddress = null)
        {
            ipAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString();
            
            if (!_geolocationService.IsValidIpAddress(ipAddress))
                return BadRequest("Invalid IP address format");

            try
            {
                var result = await _geolocationService.GetIpDetailsAsync(ipAddress);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to lookup IP: {ex.Message}");
            }
        }

        [HttpGet("check-block")]
        public async Task<IActionResult> CheckBlock([FromQuery] string ipAddress = null )
        {
             ipAddress??= HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress))
                return BadRequest("Could not determine IP address");

            try
            {
                var ipDetails = await _geolocationService.GetIpDetailsAsync(ipAddress);
                var isBlocked = _blockedCountriesService.IsCountryBlocked(ipDetails.CountryCode);

                _loggingService.AddLog(new LogEntry
                {
                    IpAddress = ipAddress,
                    CountryCode = ipDetails.CountryCode,
                    IsBlocked = isBlocked,
                    Timestamp = DateTime.UtcNow,
                    RequestPath = HttpContext.Request.Path,
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                });

                return Ok(new
                {
                    IpAddress = ipAddress,
                    CountryCode = ipDetails.CountryCode,
                    CountryName = ipDetails.CountryName,
                    IsBlocked = isBlocked
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to check block status: {ex.Message}");
            }
        }
    }
} 