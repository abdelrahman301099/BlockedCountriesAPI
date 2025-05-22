using Microsoft.AspNetCore.Mvc;
using assignment.Services;
using assignment.Models;

namespace assignment.Controllers
{
    [ApiController]
    [Route("api/countries")]
    public class CountriesController : ControllerBase
    {
        private readonly BlockedCountriesService _blockedCountriesService;

        public CountriesController(BlockedCountriesService blockedCountriesService)
        {
            _blockedCountriesService = blockedCountriesService;
        }

        [HttpPost("block")]
        public IActionResult BlockCountry([FromBody] CountryInfo country)
        {
            if (string.IsNullOrEmpty(country?.Code))
                return BadRequest("Country code is required");

            if (_blockedCountriesService.IsCountryBlocked(country.Code))
                return Conflict($"Country {country.Code} is already blocked");

            var success = _blockedCountriesService.BlockCountry(country.Code, country.Name);
            if (!success)
                return BadRequest("Failed to block country");

            return Ok();
        }

        [HttpDelete("block/{countryCode}")]
        public IActionResult UnblockCountry(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
                return BadRequest("Country code is required");

            if (!_blockedCountriesService.IsCountryBlocked(countryCode))
                return NotFound($"Country {countryCode} is not blocked");

            var success = _blockedCountriesService.UnblockCountry(countryCode);
            if (!success)
                return BadRequest("Failed to unblock country");

            return Ok();
        }

        [HttpGet("blocked")]
        public IActionResult GetBlockedCountries(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var (countries, totalCount) = _blockedCountriesService.GetBlockedCountries(page, pageSize, searchTerm);


            if (totalCount == 0)
                return BadRequest("There is no blocked Countries");

            return Ok(new
            {
                Countries = countries,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        [HttpPost("temporal-block")]
        public IActionResult AddTemporalBlock([FromBody] TemporalBlockRequest request)
        {
            if (string.IsNullOrEmpty(request?.CountryCode))
                return BadRequest("Country code is required");

            if (request.DurationMinutes < 1 || request.DurationMinutes > 1440)
                return BadRequest("Duration must be between 1 and 1440 minutes");

            if (_blockedCountriesService.IsTemporarilyBlocked(request.CountryCode))
                return Conflict($"Country {request.CountryCode} is already temporarily blocked");

            var success = _blockedCountriesService.AddTemporalBlock(request.CountryCode, request.DurationMinutes);
            if (!success)
                return BadRequest("Invalid country code or failed to add temporal block");

            return Ok();
        }
    }

    public class TemporalBlockRequest
    {
        public string CountryCode { get; set; }
        public int DurationMinutes { get; set; }
    }
} 