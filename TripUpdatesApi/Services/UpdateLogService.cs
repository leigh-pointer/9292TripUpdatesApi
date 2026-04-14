using TripUpdatesApi.Data;
using TripUpdatesApi.DTOs;
using TripUpdatesApi.Models;

namespace TripUpdatesApi.Services;

// UpdateLogService handles all read operations for the update audit trail.
// It is intentionally kept separate from TripService because the two
// responsibilities — processing live updates vs querying historical logs —
// have different change rates and could be owned by different teams in a
// larger system.
public class UpdateLogService
{
    private readonly MockDatabase _db;

    public UpdateLogService(MockDatabase db) => _db = db;

    // Returns update logs matching the optional filters, projected to UpdateLogDto.
    //
    // The status parameter arrives as a raw string from the query string
    // (e.g. ?status=Late) because HTTP query parameters are always strings.
    // Enum.TryParse converts it case-insensitively to UpdateStatus, and if the
    // value is unrecognised the filter is simply ignored — a lenient approach
    // that avoids returning 400 for a typo when the intent is clearly to filter.
    public IEnumerable<UpdateLogDto> GetLogs(DateTime? from, DateTime? to, string? status)
    {
        // Attempt to parse the string status into the enum.
        // ignoreCase: true means "late", "Late", and "LATE" all work.
        UpdateStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<UpdateStatus>(status, true, out var parsed))
            statusEnum = parsed;

        // Delegate filtering and ordering to the database layer, then project
        // to the DTO to avoid leaking domain model internals to the controller.
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
