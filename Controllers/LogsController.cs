using System;
using Microsoft.AspNetCore.Mvc;
using assignment.Services;

namespace assignment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly BlockedAttemptLogService _logService;

        public LogsController(BlockedAttemptLogService logService)
        {
            _logService = logService;
        }

        [HttpGet("blocked-attempts")]
        public IActionResult GetBlockedAttempts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1)
                return BadRequest("Page number must be greater than 0");
            
            if (pageSize < 1 || pageSize > 100)
                return BadRequest("Page size must be between 1 and 100");

            var (logs, totalCount) = _logService.GetLogs(page, pageSize);
            
            return Ok(new
            {
                Logs = logs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
    }
} 