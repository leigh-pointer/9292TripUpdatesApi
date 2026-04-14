using TripUpdatesApi.Data;
using TripUpdatesApi.DTOs;
using TripUpdatesApi.Models;

namespace TripUpdatesApi.Services;

// TripService is the core business-logic layer for trip queries and real-time
// update processing.  It sits between the HTTP controllers (which only handle
// request/response concerns) and MockDatabase (which only handles data access).
//
// This separation means:
//   - Controllers stay thin and testable without needing to mock HTTP context.
//   - Business rules (the ±2-minute margin, status mapping) live in one place.
//   - Swapping MockDatabase for a real DbContext requires no changes here.
public class TripService
{
    private readonly MockDatabase _db;

    // The industry-standard tolerance for "on time" in public transit.
    // A trip is considered on time if the average deviation across departure
    // and arrival is within ±2 minutes of the scheduled times.
    // Defined as a constant so it is easy to find, change, and test.
    private const int MarginMinutes = 2;

    // Constructor injection: ASP.NET Core's DI container resolves MockDatabase
    // and passes it in.  Using a primary-constructor-style expression keeps
    // the boilerplate minimal while remaining explicit about the dependency.
    public TripService(MockDatabase db) => _db = db;

    // ── Query ─────────────────────────────────────────────────────────────────

    // Returns trips matching the optional filters, projected to TripDto.
    // The projection (Select) ensures the controller never receives a domain
    // entity — only the safe, serialisation-ready DTO.
    public IEnumerable<TripDto> GetTrips(int? lineId, DateTime? from, DateTime? to)
    {
        return _db.GetTrips(lineId, from, to)
            .Select(t => new TripDto
            {
                TripId = t.TripId,
                LineNo = t.LineNo,
                DepartureTime = t.DepartureTime,
                ArrivalTime = t.ArrivalTime,
                // ToString() converts the enum to its name ("OnTime", "Late", etc.)
                // so the JSON response is human-readable without a custom converter.
                Status = t.Status.ToString()
            });
    }

    // ── Batch update processing ───────────────────────────────────────────────

    // Processes every update in the batch, applying a try/catch per item so
    // that one bad update does not abort the entire batch.  This is the
    // "partial success" pattern: the response always reports exactly how many
    // items succeeded and how many failed, with error details for each failure.
    public BatchUpdateResult ProcessUpdates(BatchUpdateRequest request)
    {
        var result = new BatchUpdateResult { TotalProcessed = request.Updates.Count };

        foreach (var update in request.Updates)
        {
            try
            {
                // Validate that the referenced trip actually exists before
                // attempting any mutation.  Returning a descriptive error message
                // (rather than throwing) keeps the batch processing going.
                var trip = _db.GetTrip(update.TripId);
                if (trip == null)
                {
                    result.Errors.Add($"Trip {update.TripId} not found");
                    result.FailureCount++;
                    continue;
                }

                // Determine the new punctuality status by comparing actual vs
                // scheduled times using the ±2-minute margin rule.
                var newStatus = DetermineStatus(trip, update);

                // Map UpdateStatus → TripStatus.  Both enums have the same members
                // but are kept separate so the log vocabulary and the live-trip
                // vocabulary can diverge in future without a breaking change.
                var tripStatus = newStatus == UpdateStatus.Late      ? TripStatus.Late      :
                                 newStatus == UpdateStatus.Early     ? TripStatus.Early     :
                                 newStatus == UpdateStatus.OnTime    ? TripStatus.OnTime    :
                                 newStatus == UpdateStatus.Cancelled ? TripStatus.Cancelled :
                                 TripStatus.Invalid;

                // Persist the new status on the live trip record.
                _db.UpdateTripStatus(update.TripId, tripStatus);

                // Create an immutable audit log entry for this update.
                // UTC timestamp ensures consistent ordering regardless of the
                // server's local timezone.
                var log = _db.AddUpdateLog(new UpdateLog
                {
                    TripId = update.TripId,
                    UpdateTimestamp = DateTime.UtcNow,
                    Status = newStatus
                });

                // Include the created log in the response so the caller gets
                // immediate confirmation of what was recorded.
                result.Logs.Add(new UpdateLogDto
                {
                    UpdateLogId = log.UpdateLogId,
                    TripId = log.TripId,
                    UpdateTimestamp = log.UpdateTimestamp,
                    Status = log.Status.ToString()
                });
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                // Catch-all guard: if something unexpected goes wrong for one
                // update, record the error and continue processing the rest.
                result.Errors.Add($"Error processing trip {update.TripId}: {ex.Message}");
                result.FailureCount++;
            }
        }

        return result;
    }

    // ── Status determination ──────────────────────────────────────────────────

    // Core business rule: compares actual vs scheduled times and returns the
    // appropriate punctuality status.
    //
    // Algorithm:
    //   1. Calculate the deviation in minutes for departure and arrival separately.
    //      Positive = late, negative = early.
    //   2. Average the two deviations to get a single representative figure.
    //      This smooths out cases where a trip departs late but recovers time
    //      en route (or vice versa).
    //   3. Apply the ±2-minute industry tolerance:
    //      avgDiff ≤ -2  → Early
    //      avgDiff ≥ +2  → Late
    //      otherwise     → OnTime
    //
    // The method is public so it can be unit-tested directly without going
    // through the full ProcessUpdates pipeline.
    public UpdateStatus DetermineStatus(Trip trip, TripUpdateRequest update)
    {
        var departureDiff = (update.ActualDeparture - trip.DepartureTime).TotalMinutes;
        var arrivalDiff   = (update.ActualArrival   - trip.ArrivalTime).TotalMinutes;
        var avgDiff       = (departureDiff + arrivalDiff) / 2;

        if (avgDiff <= -MarginMinutes)
            return UpdateStatus.Early;
        if (avgDiff >= MarginMinutes)
            return UpdateStatus.Late;
        return UpdateStatus.OnTime;
    }
}
