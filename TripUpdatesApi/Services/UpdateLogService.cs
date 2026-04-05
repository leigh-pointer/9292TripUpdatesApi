using TripUpdatesApi.Data;
using TripUpdatesApi.DTOs;
using TripUpdatesApi.Models;

namespace TripUpdatesApi.Services;

public class UpdateLogService
{
    private readonly MockDatabase _db;

    public UpdateLogService(MockDatabase db) => _db = db;

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
