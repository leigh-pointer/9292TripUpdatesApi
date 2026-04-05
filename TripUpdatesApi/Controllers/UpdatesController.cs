using Microsoft.AspNetCore.Mvc;
using TripUpdatesApi.DTOs;
using TripUpdatesApi.Services;

namespace TripUpdatesApi.Controllers;

[ApiController]
[Route("updates")]
public class UpdatesController : ControllerBase
{
    private readonly TripService _tripService;

    public UpdatesController(TripService tripService) => _tripService = tripService;

    [HttpPost("trips")]
    public IActionResult ProcessTripUpdates([FromBody] BatchUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = _tripService.ProcessUpdates(request);
        return Ok(result);
    }
}
