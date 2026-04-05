namespace TripUpdatesApi.Models;

public class Trip
{
    public int TripId { get; set; }
    public int LineNo { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public TripStatus Status { get; set; } = TripStatus.OnTime;
}

public enum TripStatus
{
    OnTime,
    Early,
    Late,
    Cancelled,
    Invalid
}
