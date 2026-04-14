namespace TripUpdatesApi.DTOs;

// Output contracts — what the API sends back to callers.
// Kept separate from domain models so internal fields are never accidentally
// exposed, and the response shape can be versioned independently.

// Read model for GET /trips.
// Status is serialised as a string ("OnTime", "Late", etc.) rather than an
// integer so the response is self-describing without needing the enum definition.
public class TripDto
{
    public int TripId { get; set; }
    public int LineNo { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

// Read model for GET /updatelogs and for the Logs list inside BatchUpdateResult.
public class UpdateLogDto
{
    public int UpdateLogId { get; set; }
    public int TripId { get; set; }
    public DateTime UpdateTimestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}

// Response for POST /updates/trips.
// Designed around the partial-success pattern: one bad trip ID in a batch of
// ten should not fail the whole request.  The caller gets a full breakdown:
//   - counts to know at a glance how many succeeded vs failed
//   - Logs for every successful update (immediate confirmation of what was recorded)
//   - Errors for every failure with enough detail to diagnose the problem
public class BatchUpdateResult
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<UpdateLogDto> Logs { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
