using System.ComponentModel.DataAnnotations;

namespace TripUpdatesApi.DTOs;

// Input contract for POST /updates/trips.
// DTOs are kept separate from domain models so the public API shape and the
// internal domain model can evolve independently.

// What the caller must supply for a single trip update.
// The API computes the status — the caller only provides the observed times.
// This is a deliberate design choice: status determination is domain logic
// that belongs in the service, not something the caller should dictate.
public class TripUpdateRequest
{
    [Required]
    public int TripId { get; set; }

    // Actual departure as reported by the operator or vehicle computer.
    [Required]
    public DateTime ActualDeparture { get; set; }

    // Actual arrival as reported by the operator or vehicle computer.
    [Required]
    public DateTime ActualArrival { get; set; }
}

// Wraps multiple updates into a single request so one HTTP call can report
// an entire run's worth of data — typical of how vehicle computers batch
// and transmit at the end of a journey.
// [MinLength(1)] rejects empty batches at the model-binding layer before
// any business logic runs.
public class BatchUpdateRequest
{
    [Required]
    [MinLength(1)]
    public List<TripUpdateRequest> Updates { get; set; } = new();
}
