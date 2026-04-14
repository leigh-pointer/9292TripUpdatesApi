namespace TripUpdatesApi.Models;

// A Line is a named transit route operated by a specific Operator.
// In the real 9292 domain a line groups all trips that follow the same
// physical route (e.g. "Amsterdam Centraal → Utrecht Centraal").
public class Line
{
    // Surrogate primary key used internally to link trips to their line.
    public int LineId { get; set; }

    // Foreign key to Operator.OperatorNo — identifies which transit company
    // runs this line (e.g. "OP001" = Metro, "OP002" = Stad Bus).
    public string OperatorNo { get; set; } = string.Empty;

    // The official planning number assigned to this line by the operator.
    // In production this would come from a national transit data feed (e.g. BISON/KV1).
    public string LinePlanningNumber { get; set; } = string.Empty;
}
