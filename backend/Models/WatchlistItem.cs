using System.Text.Json.Serialization;

namespace Backend.Models;

public class WatchlistItem
{
    public int Id { get; set; }
    public string User { get; set; } = "Anonimo";
    public int MovieId { get; set; }
    public string Status { get; set; } = "want_to_watch";
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public Movie Movie { get; set; } = null!;
}
