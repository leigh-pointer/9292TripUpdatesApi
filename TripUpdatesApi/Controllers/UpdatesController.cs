using Microsoft.AspNetCore.Mvc;
using TripUpdatesApi.DTOs;
using TripUpdatesApi.Services;

namespace TripUpdatesApi.Controllers;

// UpdatesController handles all write operations — specifically the ingestion
// of real-time trip updates from operators.
//
// The route is "updates" (hardcoded) rather than "[controller]" because the
// class name "UpdatesController" would produce the same result, but being
// explicit makes the intent clearer and prevents accidental renames from
// silently changing the public API URL.
[ApiController]
[Route("updates")]
public class UpdatesController : ControllerBase
{
    private readonly TripService _tripService;

    public UpdatesController(TripService tripService) => _tripService = tripService;

    // POST /updates/trips
    //
    // Accepts a JSON body matching BatchUpdateRequest — a list of one or more
    // trip updates.  Using a batch endpoint (rather than one POST per trip)
    // reduces HTTP round-trips when an operator sends a burst of updates.
    //
    // [FromBody] tells the model binder to deserialise the request body as JSON.
    // Because [ApiController] is present, ModelState is validated automatically
    // before this method is called — but the explicit check is kept here as a
    // clear, visible guard for presentation purposes.
    //
    // Returns 200 OK with a BatchUpdateResult that breaks down successes,
    // failures, and the log entries created.  A 200 (rather than 207 Multi-Status)
    // is used for simplicity; in production a 207 would more accurately reflect
    // partial-success scenarios.
    [HttpPost("trips")]
    public IActionResult ProcessTripUpdates([FromBody] BatchUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = _tripService.ProcessUpdates(request);
        return Ok(result);
    }
}
