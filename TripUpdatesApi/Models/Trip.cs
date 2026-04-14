namespace TripUpdatesApi.Models;

// Trip holds two distinct things:
//   1. The scheduled timetable (DepartureTime, ArrivalTime) — immutable,
//      comes from the planning system.
//   2. The current operational status — mutable, updated by the real-time
//      update pipeline every time an operator reports actual times.
//
// Keeping both on the same entity is a simplification for this demo.
// In production the scheduled data would likely live in a separate read-only
// timetable table, with status tracked separately.
public class Trip
{
    public int TripId { get; set; }

    // The line this trip runs on.  Named LineNo to match transit operator
    // domain language rather than the generic "LineId" convention.
    public int LineNo { get; set; }

    // Planned times from the timetable — the baseline for status calculation.
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }

    // Current punctuality status.  Starts as OnTime (seeded state) and is
    // overwritten each time a TripUpdateRequest is processed.
    public TripStatus Status { get; set; } = TripStatus.OnTime;
}

// Every possible outcome of a punctuality assessment.
// Mirrors UpdateStatus on UpdateLog — both enums are kept separate so the
// live-trip vocabulary and the audit-log vocabulary can diverge independently.
public enum TripStatus
{
    OnTime,    // Within the ±2-minute industry tolerance.
    Early,     // More than 2 minutes ahead of schedule.
    Late,      // More than 2 minutes behind schedule.
    Cancelled, // Trip did not run.
    Invalid    // Actual times were inconsistent or uninterpretable.
}
