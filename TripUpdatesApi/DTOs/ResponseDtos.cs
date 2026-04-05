namespace TripUpdatesApi.DTOs;

public class TripDto
{
    public int TripId { get; set; }
    public int LineNo { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UpdateLogDto
{
    public int UpdateLogId { get; set; }
    public int TripId { get; set; }
    public DateTime UpdateTimestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class BatchUpdateResult
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<UpdateLogDto> Logs { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
