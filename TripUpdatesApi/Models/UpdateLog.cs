namespace TripUpdatesApi.Models;

public class UpdateLog
{
    public int UpdateLogId { get; set; }
    public int TripId { get; set; }
    public DateTime UpdateTimestamp { get; set; }
    public UpdateStatus Status { get; set; }
}

public enum UpdateStatus
{
    OnTime,
    Early,
    Late,
    Cancelled,
    Invalid
}
