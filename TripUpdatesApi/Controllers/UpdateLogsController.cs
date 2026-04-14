using Microsoft.AspNetCore.Mvc;
using TripUpdatesApi.Services;

namespace TripUpdatesApi.Controllers;

// UpdateLogsController exposes the audit trail of all processed trip updates.
// It is read-only by design — logs are created by the update pipeline and
// should never be modified or deleted through the API.
[ApiController]
[Route("[controller]")]
public class UpdateLogsController : ControllerBase
{
    private readonly UpdateLogService _logService;

    public UpdateLogsController(UpdateLogService logService) => _logService = logService;

    // GET /updatelogs
    // GET /updatelogs?status=Late
    // GET /updatelogs?from=2026-03-27T00:00:00&to=2026-03-27T23:59:59
    //
    // All parameters are optional.  The status filter accepts a string so
    // callers can write ?status=Late instead of ?status=2, making the API
    // self-documenting.  Case-insensitive parsing is handled in UpdateLogService.
    //
    // Results are returned newest-first (ordered by UpdateTimestamp descending)
    // so the most recent activity is always at the top of the response.
    [HttpGet]
    public IActionResult GetLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status)
    {
        var logs = _logService.GetLogs(from, to, status);
        return Ok(logs);
    }
}
