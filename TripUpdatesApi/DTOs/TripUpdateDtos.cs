using System.ComponentModel.DataAnnotations;

namespace TripUpdatesApi.DTOs;

public class TripUpdateRequest
{
    [Required]
    public int TripId { get; set; }

    [Required]
    public DateTime ActualDeparture { get; set; }

    [Required]
    public DateTime ActualArrival { get; set; }
}

public class BatchUpdateRequest
{
    [Required]
    [MinLength(1)]
    public List<TripUpdateRequest> Updates { get; set; } = new();
}
