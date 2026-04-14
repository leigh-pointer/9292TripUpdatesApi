namespace TripUpdatesApi.Models;

// A Trip represents a single scheduled journey on a transit line.
// It holds both the planned timetable (DepartureTime / ArrivalTime) and the
// current operational status that gets updated as real-time data arrives.
public class Trip
{
    // Unique identifier for the trip, auto-assigned by MockDatabase.
    public int TripId { get; set; }

    // References the Line this trip runs on (matches Line.LineId).
    // Named LineNo to reflect the domain language used by transit operators.
    public int LineNo { get; set; }

    // Planned (scheduled) departure and arrival times from the timetable.
    // These are the baseline values against which actual times are compared
    // to determine whether a trip is on time, early, or late.
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }

    // Current punctuality status of the trip.
    // Defaults to OnTime when the trip is first created (seeded data).
    // Updated every time a TripUpdateRequest is processed by TripService.
    public TripStatus Status { get; set; } = TripStatus.OnTime;
}

// Represents every possible punctuality outcome for a trip.
// Mirrors UpdateStatus (on UpdateLog) so both the live trip record and the
// audit log use the same vocabulary.
public enum TripStatus
{
    OnTime,    // Actual times are within the ±2-minute industry tolerance.
    Early,     // Trip arrived/departed more than 2 minutes ahead of schedule.
    Late,      // Trip arrived/departed more than 2 minutes behind schedule.
    Cancelled, // Trip did not run.
    Invalid    // Update data was inconsistent or could not be interpreted.
}
