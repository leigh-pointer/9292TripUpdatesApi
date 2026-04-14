using TripUpdatesApi.Models;

namespace TripUpdatesApi.Data;

// MockDatabase is the in-memory data store for the entire application.
// It replaces a real database (PostgreSQL / SQL Server) for this demo so that
// the project has zero external dependencies and can be run with a single
// `dotnet run`.
//
// Registered as a Singleton in Program.cs so every request shares the same
// instance — this is what gives the illusion of persistence within a single
// process lifetime.  Restarting the app resets all data to the seeded state.
//
// In production this class would be replaced by:
//   - EF Core DbContext with real migrations
//   - Repository pattern to abstract data access behind interfaces
//   - A connection string pointing to a persistent database
public class MockDatabase
{
    // In-memory collections that act as database tables.
    // Public setters allow WebApplicationFactory to swap data in integration tests.
    public List<Trip> Trips { get; set; } = new();
    public List<Line> Lines { get; set; } = new();
    public List<Operator> Operators { get; set; } = new();
    public List<UpdateLog> UpdateLogs { get; set; } = new();

    // Simple auto-increment counters that mimic a database IDENTITY column.
    // Starting at 1 matches the convention of most relational databases.
    private int _tripIdCounter = 1;
    private int _logIdCounter = 1;

    // The constructor immediately seeds the database so the API is usable
    // the moment it starts — no manual setup or migration step required.
    public MockDatabase()
    {
        SeedData();
    }

    // ── Seed data ─────────────────────────────────────────────────────────────
    // Populates the in-memory tables with realistic-looking transit data.
    // Using tomorrow's date (AddDays(1)) ensures the scheduled times are always
    // in the future, which makes the demo data feel live regardless of when
    // the app is started.
    private void SeedData()
    {
        // Three lines across two operators to demonstrate the filtering capability
        // of GET /trips?lineId= and the operator relationship.
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

        var baseDate = DateTime.Today.AddDays(1);

        // Five trips spread across the three lines with varying departure windows.
        // All start as OnTime — their status changes when POST /updates/trips is called.
        Trips.AddRange(new[]
        {
            new Trip { TripId = _tripIdCounter++, LineNo = 1, DepartureTime = baseDate.AddHours(8),  ArrivalTime = baseDate.AddHours(9),                   Status = TripStatus.OnTime },
            new Trip { TripId = _tripIdCounter++, LineNo = 1, DepartureTime = baseDate.AddHours(10), ArrivalTime = baseDate.AddHours(11),                  Status = TripStatus.OnTime },
            new Trip { TripId = _tripIdCounter++, LineNo = 2, DepartureTime = baseDate.AddHours(9),  ArrivalTime = baseDate.AddHours(10).AddMinutes(30),   Status = TripStatus.OnTime },
            new Trip { TripId = _tripIdCounter++, LineNo = 2, DepartureTime = baseDate.AddHours(12), ArrivalTime = baseDate.AddHours(13).AddMinutes(30),   Status = TripStatus.OnTime },
            new Trip { TripId = _tripIdCounter++, LineNo = 3, DepartureTime = baseDate.AddHours(14), ArrivalTime = baseDate.AddHours(15),                  Status = TripStatus.OnTime }
        });
    }

    // ── Query methods ─────────────────────────────────────────────────────────
    // These methods encapsulate all data-access logic, keeping the service layer
    // free of collection-manipulation details.  In production each of these
    // would be a LINQ-to-EF query that translates to SQL.

    // Single-trip lookup used by TripService when processing an update.
    // Returns null if the trip does not exist so the caller can handle the
    // "not found" case explicitly rather than catching an exception.
    public Trip? GetTrip(int tripId) => Trips.FirstOrDefault(t => t.TripId == tripId);

    // Filtered trip query supporting three independent, optional filters.
    // All three filters are AND-ed together: only trips matching every
    // supplied criterion are returned.  Omitting a filter returns all trips
    // for that dimension.
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

    // Filtered update-log query with the same optional-filter pattern.
    // Results are ordered newest-first so callers see the most recent activity
    // at the top without needing to sort themselves.
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

    // ── Mutation methods ──────────────────────────────────────────────────────

    // Appends a new log entry and assigns it a unique ID before returning it.
    // The caller receives the fully-populated log (with its new ID) so it can
    // immediately include it in the API response without a second lookup.
    public UpdateLog AddUpdateLog(UpdateLog log)
    {
        log.UpdateLogId = _logIdCounter++;
        UpdateLogs.Add(log);
        return log;
    }

    // Updates the live status of a trip in place.
    // The null-guard is a safety net; in practice the service always verifies
    // the trip exists before calling this method.
    public void UpdateTripStatus(int tripId, TripStatus status)
    {
        var trip = GetTrip(tripId);
        if (trip != null)
            trip.Status = status;
    }
}
