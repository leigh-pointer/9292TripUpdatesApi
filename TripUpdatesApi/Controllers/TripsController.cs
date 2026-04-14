using Microsoft.AspNetCore.Mvc;
using TripUpdatesApi.Services;

namespace TripUpdatesApi.Controllers;

// TripsController exposes the read-only trip catalogue.
//
// [ApiController] enables several conventions automatically:
//   - Automatic HTTP 400 response when ModelState is invalid.
//   - Binding source inference ([FromQuery], [FromBody] etc. inferred from context).
//   - Problem Details (RFC 7807) formatted error responses.
//
// [Route("[controller]")] resolves to "/trips" at runtime using the class name
// minus the "Controller" suffix — a convention that keeps route and class name
// in sync without hardcoding the path.
[ApiController]
[Route("[controller]")]
public class TripsController : ControllerBase
{
    private readonly TripService _tripService;

    // TripService is injected by the DI container.  The controller has no
    // knowledge of MockDatabase — it only talks to the service layer.
    public TripsController(TripService tripService) => _tripService = tripService;

    // GET /trips
    // GET /trips?lineId=1
    // GET /trips?from=2026-03-27T08:00:00&to=2026-03-27T12:00:00
    //
    // All three query parameters are optional (nullable).  When omitted the
    // service returns all trips.  [FromQuery] tells the model binder to read
    // each parameter from the URL query string rather than the route or body.
    //
    // Returns 200 OK with a JSON array of TripDto objects.
    // An empty array (no matching trips) is still a 200 — "not found" applies
    // to individual resources, not collections.
    [HttpGet]
    public IActionResult GetTrips([FromQuery] int? lineId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var trips = _tripService.GetTrips(lineId, from, to);
        return Ok(trips);
    }
}
