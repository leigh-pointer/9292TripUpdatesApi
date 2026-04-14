namespace TripUpdatesApi.Models;

// UpdateLog is the audit trail — an append-only record of every status decision
// the system has made.  It answers: "what status was assigned to trip X,
// and exactly when was that decision made?"
//
// This is intentionally separate from Trip:
//   - Trip = current state (one record per trip, overwritten on each update)
//   - UpdateLog = full history (one record per update, never modified)
//
// In production this would be an append-only table with no UPDATE or DELETE
// permissions granted to the application role.
public class UpdateLog
{
    public int UpdateLogId { get; set; }

    // Which trip this decision was about.
    public int TripId { get; set; }

    // When the API processed the update — stored in UTC to avoid timezone
    // ambiguity across operators in different regions.
    public DateTime UpdateTimestamp { get; set; }

    // The status that was calculated and written to the trip at this moment.
    public UpdateStatus Status { get; set; }
}

// Mirrors TripStatus but kept separate so the log vocabulary can evolve
// independently — e.g. adding a "Delayed" sub-status to logs without
// changing the live trip status enum.
public enum UpdateStatus
{
    OnTime,    // Within ±2 minutes of schedule.
    Early,     // More than 2 minutes ahead of schedule.
    Late,      // More than 2 minutes behind schedule.
    Cancelled, // Trip was cancelled.
    Invalid    // Update could not be interpreted.
}
