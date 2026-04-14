using Microsoft.AspNetCore.Mvc;
using TripUpdatesApi.Services;

namespace TripUpdatesApi.Controllers;

// Thin HTTP layer for the trip catalogue.
// Responsibility: translate query-string parameters into a service call and
// return the result.  No business logic lives here.
[ApiController]
[Route("[controller]")]
public class TripsController : ControllerBase
{
    private readonly TripService _tripService;

    public TripsController(TripService tripService) => _tripService = tripService;

    // GET /trips
    // GET /trips?lineId=1
    // GET /trips?from=2026-03-27T08:00:00&to=2026-03-27T12:00:00
    // All filters are optional and combinable.  An empty result is still 200 —
    // 404 applies to individual resources, not collections.
    [HttpGet]
    public IActionResult GetTrips([FromQuery] int? lineId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var trips = _tripService.GetTrips(lineId, from, to);
        return Ok(trips);
    }
}
