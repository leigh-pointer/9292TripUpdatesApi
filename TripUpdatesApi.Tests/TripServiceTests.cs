using TripUpdatesApi.Data;
using TripUpdatesApi.DTOs;
using TripUpdatesApi.Models;
using TripUpdatesApi.Services;

namespace TripUpdatesApi.Tests;

public class TripServiceTests
{
    private readonly MockDatabase _db;
    private readonly TripService _service;

    public TripServiceTests()
    {
        _db = new MockDatabase();
        _service = new TripService(_db);
    }

    [Fact]
    public void DetermineStatus_OnTime_ReturnsOnTimeWithinMargin()
    {
        // Arrange
        var trip = new Trip { TripId = 1, DepartureTime = DateTime.Today.AddHours(8), ArrivalTime = DateTime.Today.AddHours(9) };
        var update = new TripUpdateRequest { TripId = 1, ActualDeparture = trip.DepartureTime, ActualArrival = trip.ArrivalTime };

        // Act
        var status = _service.DetermineStatus(trip, update);

        // Assert
        Assert.Equal(UpdateStatus.OnTime, status);
    }

    [Fact]
    public void DetermineStatus_Late_ReturnsLateWhenExceedsMargin()
    {
        // Arrange
        var trip = new Trip { TripId = 1, DepartureTime = DateTime.Today.AddHours(8), ArrivalTime = DateTime.Today.AddHours(9) };
        var update = new TripUpdateRequest { TripId = 1, ActualDeparture = trip.DepartureTime.AddMinutes(5), ActualArrival = trip.ArrivalTime.AddMinutes(5) };

        // Act
        var status = _service.DetermineStatus(trip, update);

        // Assert
        Assert.Equal(UpdateStatus.Late, status);
    }

    [Fact]
    public void DetermineStatus_Early_ReturnsEarlyWhenBelowMargin()
    {
        // Arrange
        var trip = new Trip { TripId = 1, DepartureTime = DateTime.Today.AddHours(8), ArrivalTime = DateTime.Today.AddHours(9) };
        var update = new TripUpdateRequest { TripId = 1, ActualDeparture = trip.DepartureTime.AddMinutes(-5), ActualArrival = trip.ArrivalTime.AddMinutes(-5) };

        // Act
        var status = _service.DetermineStatus(trip, update);

        // Assert
        Assert.Equal(UpdateStatus.Early, status);
    }

    [Fact]
    public void ProcessUpdates_InvalidTripId_ReportsFailure()
    {
        // Arrange
        var request = new BatchUpdateRequest
        {
            Updates = new List<TripUpdateRequest>
            {
                new() { TripId = 999, ActualDeparture = DateTime.Now, ActualArrival = DateTime.Now.AddHours(1) }
            }
        };

        // Act
        var result = _service.ProcessUpdates(request);

        // Assert
        Assert.Equal(1, result.TotalProcessed);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.Contains("not found", result.Errors[0]);
    }

    [Fact]
    public void ProcessUpdates_ValidUpdate_SuccessAndLogsCreated()
    {
        // Arrange
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

        // Assert
        Assert.Equal(1, result.TotalProcessed);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.Logs.Count);
        Assert.Equal("Late", result.Logs[0].Status);
    }

    [Fact]
    public void GetTrips_WithLineFilter_ReturnsFilteredResults()
    {
        // Act
        var trips = _service.GetTrips(lineId: 1, null, null).ToList();

        // Assert
        Assert.All(trips, t => Assert.Equal(1, t.LineNo));
    }
}
