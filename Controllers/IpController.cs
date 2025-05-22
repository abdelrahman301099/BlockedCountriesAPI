using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using assignment.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static System.Net.WebRequestMethods;

namespace assignment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IpController : ControllerBase
    {
        private readonly BlockedCountriesService _blockedCountriesService;
        private readonly BlockedAttemptLogService _logService;
        private readonly IpGeolocationService _ipGeolocationService;

        public IpController(
            BlockedCountriesService blockedCountriesService,
            BlockedAttemptLogService logService,
            IpGeolocationService ipGeolocationService)
        {
            _blockedCountriesService = blockedCountriesService;
            _logService = logService;
            _ipGeolocationService = ipGeolocationService;
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> LookupIp([FromQuery] string ipAddress = null)
        {
            ipAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString();
            
            if (string.IsNullOrEmpty(ipAddress))
            {
                return BadRequest("Could not determine IP address");
            }

            if (!_ipGeolocationService.IsValidIpAddress(ipAddress))
            {
                return BadRequest("Invalid IP address format");
            }

            try
            {
                var ipDetails = await _ipGeolocationService.GetIpDetailsAsync(ipAddress);
                return Ok(ipDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to lookup IP: {ex.Message}");
            }
        }

        [HttpGet("check-block")]
        public async Task<IActionResult> CheckBlock()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString(); // msh sh3'al m3 Localhost

            ipAddress = (ipAddress == "::1" || ipAddress == "127.0.0.1") ? "8.8.8.8" : ipAddress; // ���� �� localhost ����� ipAddress ��� ����


            //var ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            //  ?? HttpContext.Connection.RemoteIpAddress?.ToString();



            if (string.IsNullOrEmpty(ipAddress))
            {
                return BadRequest("Could not determine IP address");
            }

            var countryCode = await _ipGeolocationService.GetCountryCodeFromIp(ipAddress);
            if (string.IsNullOrEmpty(countryCode))
            {
                return BadRequest("Could not determine country code");
            }

            var isBlocked = _blockedCountriesService.IsCountryBlocked(countryCode);
            
            
            _logService.LogAttempt(
                ipAddress,
                countryCode,
                isBlocked,
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            return Ok(new { IsBlocked = isBlocked, CountryCode = countryCode });
        }

       
    }
} 