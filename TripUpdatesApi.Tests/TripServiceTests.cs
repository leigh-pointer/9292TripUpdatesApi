using TripUpdatesApi.Data;
using TripUpdatesApi.DTOs;
using TripUpdatesApi.Models;
using TripUpdatesApi.Services;

namespace TripUpdatesApi.Tests;

// Unit tests for TripService — the class that contains all core business logic.
// These tests are pure unit tests: they instantiate real objects (no mocks)
// because MockDatabase has no external dependencies and is fast to construct.
// This approach is sometimes called "sociable unit testing" — the test covers
// TripService together with MockDatabase, but nothing outside the process.
public class TripServiceTests
{
    // A fresh MockDatabase and TripService are created for each test via the
    // constructor.  xUnit creates a new instance of the test class per test
    // method, so every test starts with clean, seeded data — no shared state
    // that could cause tests to interfere with each other.
    private readonly MockDatabase _db;
    private readonly TripService _service;

    public TripServiceTests()
    {
        _db = new MockDatabase();
        _service = new TripService(_db);
    }

    // ── DetermineStatus tests ─────────────────────────────────────────────────
    // These three tests verify the ±2-minute margin rule in isolation by calling
    // DetermineStatus directly (it is public for exactly this reason).

    [Fact]
    public void DetermineStatus_OnTime_ReturnsOnTimeWithinMargin()
    {
        // Arrange: actual times exactly match scheduled times (0-minute deviation).
        var trip = new Trip { TripId = 1, DepartureTime = DateTime.Today.AddHours(8), ArrivalTime = DateTime.Today.AddHours(9) };
        var update = new TripUpdateRequest { TripId = 1, ActualDeparture = trip.DepartureTime, ActualArrival = trip.ArrivalTime };

        // Act
        var status = _service.DetermineStatus(trip, update);

        // Assert: 0 minutes deviation is within the ±2-minute window → OnTime.
        Assert.Equal(UpdateStatus.OnTime, status);
    }

    [Fact]
    public void DetermineStatus_Late_ReturnsLateWhenExceedsMargin()
    {
        // Arrange: actual times are 5 minutes late — clearly outside the margin.
        var trip = new Trip { TripId = 1, DepartureTime = DateTime.Today.AddHours(8), ArrivalTime = DateTime.Today.AddHours(9) };
        var update = new TripUpdateRequest { TripId = 1, ActualDeparture = trip.DepartureTime.AddMinutes(5), ActualArrival = trip.ArrivalTime.AddMinutes(5) };

        // Act
        var status = _service.DetermineStatus(trip, update);

        // Assert: +5 minutes average deviation exceeds +2 → Late.
        Assert.Equal(UpdateStatus.Late, status);
    }

    [Fact]
    public void DetermineStatus_Early_ReturnsEarlyWhenBelowMargin()
    {
        // Arrange: actual times are 5 minutes early — outside the margin on the other side.
        var trip = new Trip { TripId = 1, DepartureTime = DateTime.Today.AddHours(8), ArrivalTime = DateTime.Today.AddHours(9) };
        var update = new TripUpdateRequest { TripId = 1, ActualDeparture = trip.DepartureTime.AddMinutes(-5), ActualArrival = trip.ArrivalTime.AddMinutes(-5) };

        // Act
        var status = _service.DetermineStatus(trip, update);

        // Assert: -5 minutes average deviation is below -2 → Early.
        Assert.Equal(UpdateStatus.Early, status);
    }

    // ── ProcessUpdates tests ──────────────────────────────────────────────────

    [Fact]
    public void ProcessUpdates_InvalidTripId_ReportsFailure()
    {
        // Arrange: trip ID 999 does not exist in the seeded database.
        var request = new BatchUpdateRequest
        {
            Updates = new List<TripUpdateRequest>
            {
                new() { TripId = 999, ActualDeparture = DateTime.Now, ActualArrival = DateTime.Now.AddHours(1) }
            }
        };

        // Act
        var result = _service.ProcessUpdates(request);

        // Assert: the batch processed 1 item, 0 succeeded, 1 failed, and the
        // error message contains "not found" to confirm the right code path ran.
        Assert.Equal(1, result.TotalProcessed);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.Contains("not found", result.Errors[0]);
    }

    [Fact]
    public void ProcessUpdates_ValidUpdate_SuccessAndLogsCreated()
    {
        // Arrange: use the first seeded trip so we know it exists.
        // Send it 5 minutes late to get a predictable "Late" status.
        var trip = _db.Trips.First();
        var request = new BatchUpdateRequest
        {
            Updates = new List<TripUpdateRequest>
            {
                new() { TripId = trip.TripId, ActualDeparture = trip.DepartureTime.AddMinutes(5), ActualArrival = trip.ArrivalTime.AddMinutes(5) }
            }
        };

        // Act
        var result = _service.ProcessUpdates(request);

        // Assert: 1 success, 1 log created, and the log status is "Late".
        // This verifies the full pipeline: status determination → DB mutation → log creation → DTO projection.
        Assert.Equal(1, result.TotalProcessed);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.Logs.Count);
        Assert.Equal("Late", result.Logs[0].Status);
    }

    // ── GetTrips filter test ──────────────────────────────────────────────────

    [Fact]
    public void GetTrips_WithLineFilter_ReturnsFilteredResults()
    {
        // Act: request only trips on line 1.
        var trips = _service.GetTrips(lineId: 1, null, null).ToList();

        // Assert: every returned trip must belong to line 1.
        // Assert.All is xUnit's way of asserting a condition holds for every
        // element in a collection — equivalent to a foreach with an assertion inside.
        Assert.All(trips, t => Assert.Equal(1, t.LineNo));
    }
}
