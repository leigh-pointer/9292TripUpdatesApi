using TripUpdatesApi.Models;

namespace TripUpdatesApi.Data;

public class MockDatabase
{
    public List<Trip> Trips { get; set; } = new();
    public List<Line> Lines { get; set; } = new();
    public List<Operator> Operators { get; set; } = new();
    public List<UpdateLog> UpdateLogs { get; set; } = new();
    private int _tripIdCounter = 1;
    private int _logIdCounter = 1;

    public MockDatabase()
    {
        SeedData();
    }

    private void SeedData()
    {
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
        Trips.AddRange(new[]
        {
            new Trip { TripId = _tripIdCounter++, LineNo = 1, DepartureTime = baseDate.AddHours(8), ArrivalTime = baseDate.AddHours(9), Status = TripStatus.OnTime },
            new Trip { TripId = _tripIdCounter++, LineNo = 1, DepartureTime = baseDate.AddHours(10), ArrivalTime = baseDate.AddHours(11), Status = TripStatus.OnTime },
            new Trip { TripId = _tripIdCounter++, LineNo = 2, DepartureTime = baseDate.AddHours(9), ArrivalTime = baseDate.AddHours(10).AddMinutes(30), Status = TripStatus.OnTime },
            new Trip { TripId = _tripIdCounter++, LineNo = 2, DepartureTime = baseDate.AddHours(12), ArrivalTime = baseDate.AddHours(13).AddMinutes(30), Status = TripStatus.OnTime },
            new Trip { TripId = _tripIdCounter++, LineNo = 3, DepartureTime = baseDate.AddHours(14), ArrivalTime = baseDate.AddHours(15), Status = TripStatus.OnTime }
        });
    }

    public Trip? GetTrip(int tripId) => Trips.FirstOrDefault(t => t.TripId == tripId);

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

    public UpdateLog AddUpdateLog(UpdateLog log)
    {
        log.UpdateLogId = _logIdCounter++;
        UpdateLogs.Add(log);
        return log;
    }

    public void UpdateTripStatus(int tripId, TripStatus status)
    {
        var trip = GetTrip(tripId);
        if (trip != null)
            trip.Status = status;
    }
}
