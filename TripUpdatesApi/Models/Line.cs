namespace TripUpdatesApi.Models;

public class Line
{
    public int LineId { get; set; }
    public string OperatorNo { get; set; } = string.Empty;
    public string LinePlanningNumber { get; set; } = string.Empty;
}
