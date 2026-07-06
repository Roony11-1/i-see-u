using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Endpoints;

public static class WatchlistEndpoints
{
    public static void MapWatchlistEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/watchlist");

        group.MapGet("/", async (AppDbContext db) =>
        {
            return await db.WatchlistItems
                .Include(w => w.Movie)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        });

        group.MapPost("/", async (WatchlistItem item, AppDbContext db) =>
        {
            item.AddedAt = DateTime.UtcNow;
            db.WatchlistItems.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/api/watchlist/{item.Id}", item);
        });

        group.MapPut("/{id}", async (int id, string status, AppDbContext db) =>
        {
            var item = await db.WatchlistItems.FindAsync(id);
            if (item is null) return Results.NotFound();

            item.Status = status;
            await db.SaveChangesAsync();
            return Results.Ok(item);
        });

        group.MapDelete("/{id}", async (int id, AppDbContext db) =>
        {
            var item = await db.WatchlistItems.FindAsync(id);
            if (item is null) return Results.NotFound();

            db.WatchlistItems.Remove(item);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
