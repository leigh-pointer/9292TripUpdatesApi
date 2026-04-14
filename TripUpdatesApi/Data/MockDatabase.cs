using TripUpdatesApi.Models;

namespace TripUpdatesApi.Data;

// MockDatabase is the single in-memory store for the entire application.
// It has two responsibilities:
//   1. Seed realistic transit data so the API is immediately usable.
//   2. Provide the query and mutation methods that the service layer calls.
//
// This is a deliberate trade-off: no external dependencies, instant startup,
// easy to reason about during a demo.  The cost is that data resets on restart.
//
// In production this is replaced by an EF Core DbContext backed by a real
// database, with the repository pattern abstracting the data access behind
// an interface so the service layer never changes.
public class MockDatabase
{
    // These collections are the "tables". Public setters let integration tests
    // inspect or replace data without needing a real database connection.
    public List<Trip> Trips { get; set; } = new();
    public List<Line> Lines { get; set; } = new();
    public List<Operator> Operators { get; set; } = new();
    public List<UpdateLog> UpdateLogs { get; set; } = new();

    // Auto-increment counters that mimic a database IDENTITY / SERIAL column.
    private int _tripIdCounter = 1;
    private int _logIdCounter = 1;

    // Seed on construction so the API has data the moment it starts —
    // no migration, no setup script, no external dependency.
    public MockDatabase()
    {
        SeedData();
    }

    private void SeedData()
    {
        // Two operators running three lines — enough variety to demonstrate
        // the lineId filter on GET /trips without overwhelming the demo.
        Lines.AddRange(new[]
        {
            new Line { LineId = 1, OperatorNo = "OP001", LinePlanningNumber = "LN001" },
            new Line { LineId = 2, OperatorNo = "OP002", LinePlanningNumber = "LN002" },
            new Line { LineId = 3, OperatorNo = "OP001", LinePlanningNumber = "LN003" }
        });

        Operators.AddRange(new[]
        {
            new Operator { OperatorNo = "OP001", Name = "Metro" },
            new Operator { OperatorNo = "OP002", Name = "Stad Bus" }
        });

        // Tomorrow's date keeps scheduled times in the future regardless of
        // when the app is started, so the demo always feels live.
        var baseDate = DateTime.Today.AddDays(1);

        // All trips start as OnTime. Status changes the moment a real-time
        // update is POSTed — that transition is the core of the demo.
        Trips.AddRange(new[]
        {
            new Trip { TripId = _tripIdCounter++, LineNo = 1, DepartureTime = baseDate.AddHours(8),  ArrivalTime = baseDate.AddHours(9),                   Status = TripStatus.OnTime },
            new Trip { TripId = _tripIdCounter++, LineNo = 1, DepartureTime = baseDate.AddHours(10), ArrivalTime = baseDate.AddHours(11),                  Status = TripStatus.OnTime },
            new Trip { TripId = _tripIdCounter++, LineNo = 2, DepartureTime = baseDate.AddHours(9),  ArrivalTime = baseDate.AddHours(10).AddMinutes(30),   Status = TripStatus.OnTime },
            new Trip { TripId = _tripIdCounter++, LineNo = 2, DepartureTime = baseDate.AddHours(12), ArrivalTime = baseDate.AddHours(13).AddMinutes(30),   Status = TripStatus.OnTime },
            new Trip { TripId = _tripIdCounter++, LineNo = 3, DepartureTime = baseDate.AddHours(14), ArrivalTime = baseDate.AddHours(15),                  Status = TripStatus.OnTime }
        });
    }

    // ── Queries ───────────────────────────────────────────────────────────────
    // All data-access logic lives here, keeping the service layer free of
    // collection details.  In production each method becomes a LINQ-to-EF
    // query that the ORM translates to SQL.

    // Used by TripService before processing an update — returns null so the
    // caller can report a clean "not found" error rather than throw.
    public Trip? GetTrip(int tripId) => Trips.FirstOrDefault(t => t.TripId == tripId);

    // All three filters are optional and AND-ed together.
    // Omitting all of them returns the full trip catalogue.
    public IEnumerable<Trip> GetTrips(int? lineId, DateTime? from, DateTime? to)
    {
        var query = Trips.AsEnumerable();
        if (lineId.HasValue)
            query = query.Where(t => t.LineNo == lineId.Value);
        if (from.HasValue)
            query = query.Where(t => t.DepartureTime >= from.Value);
        if (to.HasValue)
            query = query.Where(t => t.DepartureTime <= to.Value);
        return query;
    }

    // Same optional-filter pattern as GetTrips.
    // Ordered newest-first so the most recent activity surfaces at the top.
    public IEnumerable<UpdateLog> GetUpdateLogs(DateTime? from, DateTime? to, UpdateStatus? status)
    {
        var query = UpdateLogs.AsEnumerable();
        if (from.HasValue)
            query = query.Where(l => l.UpdateTimestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.UpdateTimestamp <= to.Value);
        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);
        return query.OrderByDescending(l => l.UpdateTimestamp);
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    // Appends a new audit log entry and assigns its ID before returning it,
    // so TripService can include the populated log in the response immediately.
    public UpdateLog AddUpdateLog(UpdateLog log)
    {
        log.UpdateLogId = _logIdCounter++;
        UpdateLogs.Add(log);
        return log;
    }

    // Updates the live status of a trip in place.
    // TripService always verifies the trip exists first; the null-guard here
    // is a defensive safety net.
    public void UpdateTripStatus(int tripId, TripStatus status)
    {
        var trip = GetTrip(tripId);
        if (trip != null)
            trip.Status = status;
    }
}
