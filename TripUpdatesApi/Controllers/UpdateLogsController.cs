using Microsoft.AspNetCore.Mvc;
using TripUpdatesApi.Services;

namespace TripUpdatesApi.Controllers;

// Thin HTTP layer for querying the audit trail.
// Read-only by design — logs are created by the update pipeline and must
// never be modified or deleted through the API.
[ApiController]
[Route("[controller]")]
public class UpdateLogsController : ControllerBase
{
    private readonly UpdateLogService _logService;

    public UpdateLogsController(UpdateLogService logService) => _logService = logService;

    // GET /updatelogs
    // GET /updatelogs?status=Late
    // GET /updatelogs?from=2026-03-27T00:00:00&to=2026-03-27T23:59:59
    // Results are newest-first so the most recent activity is always at the top.
    [HttpGet]
    public IActionResult GetLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status)
    {
        var logs = _logService.GetLogs(from, to, status);
        return Ok(logs);
    }
}
