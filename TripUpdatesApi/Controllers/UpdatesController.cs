using Microsoft.AspNetCore.Mvc;
using TripUpdatesApi.DTOs;
using TripUpdatesApi.Services;

namespace TripUpdatesApi.Controllers;

// Thin HTTP layer for ingesting real-time updates from operators.
// Responsibility: validate the request shape and hand off to TripService.
// All processing logic — status determination, logging, partial failure
// handling — lives in the service, not here.
[ApiController]
[Route("updates")]
public class UpdatesController : ControllerBase
{
    private readonly TripService _tripService;

    public UpdatesController(TripService tripService) => _tripService = tripService;

    // POST /updates/trips
    // Accepts a batch of actual departure/arrival times.
    // Returns a BatchUpdateResult with per-item success/failure detail so the
    // caller knows exactly what was applied and what was rejected.
    // One invalid trip ID in the batch does not fail the whole request.
    [HttpPost("trips")]
    public IActionResult ProcessTripUpdates([FromBody] BatchUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = _tripService.ProcessUpdates(request);
        return Ok(result);
    }
}
