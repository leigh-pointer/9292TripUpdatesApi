namespace TripUpdatesApi.Models;

// Represents a transit operator — the company that physically runs the buses,
// trams, or trains on one or more Lines.
// In the Dutch public transport ecosystem operators are identified by a unique
// code (e.g. "GVB", "Connexxion") rather than a numeric ID.
public class Operator
{
    // Natural / business key for the operator, used as the foreign key in Line.
    // Using a string code (rather than an integer) matches real-world transit
    // data standards where operator codes are human-readable abbreviations.
    public string OperatorNo { get; set; } = string.Empty;

    // Human-readable display name shown in API responses and the Swagger UI.
    public string Name { get; set; } = string.Empty;
}
