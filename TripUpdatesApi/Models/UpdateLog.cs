namespace TripUpdatesApi.Models;

// An UpdateLog is an immutable audit record created every time a trip update
// is successfully processed.  It answers the question: "what status was
// assigned to trip X, and when was that decision made?"
//
// This separation from the Trip entity is intentional — a Trip holds the
// current state, while UpdateLog provides the full history of state changes.
// In production this would be stored in an append-only table.
public class UpdateLog
{
    // Auto-incremented surrogate key assigned by MockDatabase.AddUpdateLog.
    public int UpdateLogId { get; set; }

    // The trip this log entry refers to.  Denormalised here for fast querying
    // without a JOIN; in a relational DB this would be a foreign key.
    public int TripId { get; set; }

    // UTC timestamp of when the update was processed by the API.
    // Using UTC avoids timezone ambiguity in a system that could receive
    // updates from operators in different regions.
    public DateTime UpdateTimestamp { get; set; }

    // The punctuality status that was calculated and applied to the trip
    // at the time of this update.
    public UpdateStatus Status { get; set; }
}

// Mirrors TripStatus but lives on the log side of the model.
// Keeping them as separate enums means the log vocabulary can evolve
// independently from the live trip status vocabulary if requirements change.
public enum UpdateStatus
{
    OnTime,    // Actual times within ±2 minutes of schedule.
    Early,     // More than 2 minutes ahead of schedule.
    Late,      // More than 2 minutes behind schedule.
    Cancelled, // Trip was cancelled.
    Invalid    // Update could not be interpreted (e.g. arrival before departure).
}
