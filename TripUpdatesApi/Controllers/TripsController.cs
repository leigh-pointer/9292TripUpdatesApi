using Microsoft.AspNetCore.Mvc;
using TripUpdatesApi.Services;

namespace TripUpdatesApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TripsController : ControllerBase
{
    private readonly TripService _tripService;

    public TripsController(TripService tripService) => _tripService = tripService;

    [HttpGet]
    public IActionResult GetTrips([FromQuery] int? lineId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var trips = _tripService.GetTrips(lineId, from, to);
        return Ok(trips);
    }
}
