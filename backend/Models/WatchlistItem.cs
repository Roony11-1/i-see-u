namespace Backend.Models;

public class WatchlistItem
{
    public int Id { get; set; }
    public string User { get; set; } = "Anonimo";
    public int MovieId { get; set; }
    public string Status { get; set; } = "want_to_watch";
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public Movie Movie { get; set; } = null!;
}
