using Microsoft.AspNetCore.Mvc;
using TripUpdatesApi.Services;

namespace TripUpdatesApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UpdateLogsController : ControllerBase
{
    private readonly UpdateLogService _logService;

    public UpdateLogsController(UpdateLogService logService) => _logService = logService;

    [HttpGet]
    public IActionResult GetLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status)
    {
        var logs = _logService.GetLogs(from, to, status);
        return Ok(logs);
    }
}
