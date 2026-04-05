using TripUpdatesApi.Data;
using TripUpdatesApi.DTOs;
using TripUpdatesApi.Models;

namespace TripUpdatesApi.Services;

public class TripService
{
    private readonly MockDatabase _db;
    private const int MarginMinutes = 2;

    public TripService(MockDatabase db) => _db = db;

    public IEnumerable<TripDto> GetTrips(int? lineId, DateTime? from, DateTime? to)
    {
        return _db.GetTrips(lineId, from, to)
            .Select(t => new TripDto
            {
                TripId = t.TripId,
                LineNo = t.LineNo,
                DepartureTime = t.DepartureTime,
                ArrivalTime = t.ArrivalTime,
                Status = t.Status.ToString()
            });
    }

    public BatchUpdateResult ProcessUpdates(BatchUpdateRequest request)
    {
        var result = new BatchUpdateResult { TotalProcessed = request.Updates.Count };

        foreach (var update in request.Updates)
        {
            try
            {
                var trip = _db.GetTrip(update.TripId);
                if (trip == null)
                {
                    result.Errors.Add($"Trip {update.TripId} not found");
                    result.FailureCount++;
                    continue;
                }

                var newStatus = DetermineStatus(trip, update);
                var tripStatus = newStatus == UpdateStatus.Late ? TripStatus.Late :
                                 newStatus == UpdateStatus.Early ? TripStatus.Early :
                                 newStatus == UpdateStatus.OnTime ? TripStatus.OnTime :
                                 newStatus == UpdateStatus.Cancelled ? TripStatus.Cancelled :
                                 TripStatus.Invalid;
                _db.UpdateTripStatus(update.TripId, tripStatus);

                var log = _db.AddUpdateLog(new UpdateLog
                {
                    TripId = update.TripId,
                    UpdateTimestamp = DateTime.UtcNow,
                    Status = newStatus
                });

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
                result.Errors.Add($"Error processing trip {update.TripId}: {ex.Message}");
                result.FailureCount++;
            }
        }

        return result;
    }

    public UpdateStatus DetermineStatus(Trip trip, TripUpdateRequest update)
    {
        var departureDiff = (update.ActualDeparture - trip.DepartureTime).TotalMinutes;
        var arrivalDiff = (update.ActualArrival - trip.ArrivalTime).TotalMinutes;
        var avgDiff = (departureDiff + arrivalDiff) / 2;

        if (avgDiff <= -MarginMinutes)
            return UpdateStatus.Early;
        if (avgDiff >= MarginMinutes)
            return UpdateStatus.Late;
        return UpdateStatus.OnTime;
    }
}
