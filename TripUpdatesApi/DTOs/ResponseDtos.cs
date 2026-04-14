namespace TripUpdatesApi.DTOs;

// ─────────────────────────────────────────────────────────────────────────────
// DTOs in this file define the shape of data that GOES OUT of the API.
// They are the response contract and are deliberately decoupled from the
// domain models so that:
//   1. Internal fields (e.g. navigation properties, EF tracking state) are
//      never accidentally serialised into the response.
//   2. The response shape can be versioned independently of the domain model.
//   3. Enum values are serialised as human-readable strings (e.g. "Late")
//      rather than integers, making the API self-describing.
// ─────────────────────────────────────────────────────────────────────────────

// Read model for a Trip — returned by GET /trips.
// Status is a string so consumers receive "OnTime" / "Late" / "Early" etc.
// rather than a raw integer that requires knowledge of the enum definition.
public class TripDto
{
    public int TripId { get; set; }
    public int LineNo { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

// Read model for an UpdateLog entry — returned inside BatchUpdateResult and
// by GET /updatelogs.
// UpdateTimestamp is in UTC so consumers can convert to their local timezone.
public class UpdateLogDto
{
    public int UpdateLogId { get; set; }
    public int TripId { get; set; }
    public DateTime UpdateTimestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}

// Summary response returned by POST /updates/trips.
// Provides a clear breakdown of what happened during batch processing:
//   - TotalProcessed: how many updates were in the request.
//   - SuccessCount:   how many were applied successfully.
//   - FailureCount:   how many failed (trip not found, exception, etc.).
//   - Logs:           the UpdateLog records created for successful updates,
//                     so the caller can confirm what status was assigned.
//   - Errors:         human-readable error messages for each failure,
//                     indexed in the same order as the failed updates.
public class BatchUpdateResult
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<UpdateLogDto> Logs { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
