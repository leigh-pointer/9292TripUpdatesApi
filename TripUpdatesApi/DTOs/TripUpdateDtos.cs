using System.ComponentModel.DataAnnotations;

namespace TripUpdatesApi.DTOs;

// ─────────────────────────────────────────────────────────────────────────────
// DTOs (Data Transfer Objects) in this file define the shape of data that
// COMES IN to the API.  They are the API contract for the POST /updates/trips
// endpoint and are intentionally separate from the domain models so that:
//   1. The internal model can change without breaking the public API contract.
//   2. Validation attributes live here, not on domain entities.
//   3. Consumers only see the fields they need to supply.
// ─────────────────────────────────────────────────────────────────────────────

// Represents a single real-time update for one trip.
// The caller supplies the trip they are updating and the actual (observed)
// departure and arrival times.  The API then computes the status by comparing
// these against the scheduled times stored in the database.
public class TripUpdateRequest
{
    // The ID of the trip being updated.  [Required] ensures the model binder
    // rejects requests where this field is missing or null.
    [Required]
    public int TripId { get; set; }

    // The time the vehicle actually departed, as reported by the operator.
    // Compared against Trip.DepartureTime to calculate the departure deviation.
    [Required]
    public DateTime ActualDeparture { get; set; }

    // The time the vehicle actually arrived at its destination.
    // Compared against Trip.ArrivalTime to calculate the arrival deviation.
    [Required]
    public DateTime ActualArrival { get; set; }
}

// Wraps a list of TripUpdateRequests so a single HTTP call can update many
// trips atomically — a common pattern in transit systems where a vehicle
// computer sends a burst of updates at the end of a journey.
//
// [MinLength(1)] prevents callers from sending an empty batch, which would
// succeed but do nothing — a likely mistake rather than intentional behaviour.
public class BatchUpdateRequest
{
    [Required]
    [MinLength(1)]
    public List<TripUpdateRequest> Updates { get; set; } = new();
}
