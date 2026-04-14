using TripUpdatesApi.Data;
using TripUpdatesApi.DTOs;
using TripUpdatesApi.Models;
using TripUpdatesApi.Services;

namespace TripUpdatesApi.Tests;

// Unit tests for TripService — the home of all domain logic.
// No mocks needed: MockDatabase has no external dependencies so we use the
// real thing.  Each test gets a fresh instance (xUnit constructs the class
// per test) so there is no shared state between tests.
public class TripServiceTests
{
    private readonly MockDatabase _db;
    private readonly TripService _service;

    public TripServiceTests()
    {
        _db = new MockDatabase();
        _service = new TripService(_db);
    }

    // ── Core business rule: the ±2-minute margin ──────────────────────────────
    // DetermineStatus is public specifically so it can be tested in isolation
    // without going through the full batch pipeline.

    [Fact]
    public void DetermineStatus_OnTime_ReturnsOnTimeWithinMargin()
    {
        // 0-minute deviation — squarely within the ±2-minute window.
        var trip = new Trip { TripId = 1, DepartureTime = DateTime.Today.AddHours(8), ArrivalTime = DateTime.Today.AddHours(9) };
        var update = new TripUpdateRequest { TripId = 1, ActualDeparture = trip.DepartureTime, ActualArrival = trip.ArrivalTime };

        var status = _service.DetermineStatus(trip, update);

        Assert.Equal(UpdateStatus.OnTime, status);
    }

    [Fact]
    public void DetermineStatus_Late_ReturnsLateWhenExceedsMargin()
    {
        // +5 minutes on both departure and arrival → average +5 → Late.
        var trip = new Trip { TripId = 1, DepartureTime = DateTime.Today.AddHours(8), ArrivalTime = DateTime.Today.AddHours(9) };
        var update = new TripUpdateRequest { TripId = 1, ActualDeparture = trip.DepartureTime.AddMinutes(5), ActualArrival = trip.ArrivalTime.AddMinutes(5) };

        var status = _service.DetermineStatus(trip, update);

        Assert.Equal(UpdateStatus.Late, status);
    }

    [Fact]
    public void DetermineStatus_Early_ReturnsEarlyWhenBelowMargin()
    {
        // -5 minutes on both → average -5 → Early.
        var trip = new Trip { TripId = 1, DepartureTime = DateTime.Today.AddHours(8), ArrivalTime = DateTime.Today.AddHours(9) };
        var update = new TripUpdateRequest { TripId = 1, ActualDeparture = trip.DepartureTime.AddMinutes(-5), ActualArrival = trip.ArrivalTime.AddMinutes(-5) };

        var status = _service.DetermineStatus(trip, update);

        Assert.Equal(UpdateStatus.Early, status);
    }

    // ── Batch processing behaviour ────────────────────────────────────────────

    [Fact]
    public void ProcessUpdates_InvalidTripId_ReportsFailure()
    {
        // Proves the partial-success pattern: an unknown trip ID produces a
        // failure entry in the result without aborting the rest of the batch.
        var request = new BatchUpdateRequest
        {
            Updates = new List<TripUpdateRequest>
            {
                new() { TripId = 999, ActualDeparture = DateTime.Now, ActualArrival = DateTime.Now.AddHours(1) }
            }
        };

        var result = _service.ProcessUpdates(request);

        Assert.Equal(1, result.TotalProcessed);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.Contains("not found", result.Errors[0]);
    }

    [Fact]
    public void ProcessUpdates_ValidUpdate_SuccessAndLogsCreated()
    {
        // Proves the full pipeline for a happy-path update:
        // status determined → trip status mutated → audit log created → DTO returned.
        var trip = _db.Trips.First();
        var request = new BatchUpdateRequest
        {
            Updates = new List<TripUpdateRequest>
            {
                new() { TripId = trip.TripId, ActualDeparture = trip.DepartureTime.AddMinutes(5), ActualArrival = trip.ArrivalTime.AddMinutes(5) }
            }
        };

        var result = _service.ProcessUpdates(request);

        Assert.Equal(1, result.TotalProcessed);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.Logs.Count);
        Assert.Equal("Late", result.Logs[0].Status);
    }

    // ── Query filtering ───────────────────────────────────────────────────────

    [Fact]
    public void GetTrips_WithLineFilter_ReturnsFilteredResults()
    {
        // Proves the lineId filter is applied correctly end-to-end through
        // the service and into the database query.
        var trips = _service.GetTrips(lineId: 1, null, null).ToList();

        Assert.All(trips, t => Assert.Equal(1, t.LineNo));
    }
}
