using System.Text.Json.Serialization;

namespace Backend.Models;

public class Review
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string Author { get; set; } = "Anonimo";
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public Movie Movie { get; set; } = null!;
}
