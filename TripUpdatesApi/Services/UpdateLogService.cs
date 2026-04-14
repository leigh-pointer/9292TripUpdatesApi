using TripUpdatesApi.Data;
using TripUpdatesApi.DTOs;
using TripUpdatesApi.Models;

namespace TripUpdatesApi.Services;

// Handles all read operations against the audit trail.
// Kept separate from TripService because reading historical logs and processing
// live updates are distinct responsibilities with different change drivers.
public class UpdateLogService
{
    private readonly MockDatabase _db;

    public UpdateLogService(MockDatabase db) => _db = db;

    // Returns logs matching the optional filters, projected to UpdateLogDto.
    // The status parameter is a raw string from the query string — HTTP gives
    // us no better type here.  Enum.TryParse converts it case-insensitively;
    // an unrecognised value is silently ignored rather than returning a 400,
    // which is the right trade-off when the intent to filter is clear.
    public IEnumerable<UpdateLogDto> GetLogs(DateTime? from, DateTime? to, string? status)
    {
        UpdateStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<UpdateStatus>(status, true, out var parsed))
            statusEnum = parsed;

        return _db.GetUpdateLogs(from, to, statusEnum)
            .Select(l => new UpdateLogDto
            {
                UpdateLogId = l.UpdateLogId,
                TripId = l.TripId,
                UpdateTimestamp = l.UpdateTimestamp,
                Status = l.Status.ToString()
            });
    }
}
