using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/movies/{movieId}/reviews");

        group.MapGet("/", async (int movieId, AppDbContext db) =>
        {
            var movieExists = await db.Movies.AnyAsync(m => m.Id == movieId);
            if (!movieExists) return Results.NotFound();

            var reviews = await db.Reviews
                .Where(r => r.MovieId == movieId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Results.Ok(reviews);
        });

        group.MapPost("/", async (int movieId, Review review, AppDbContext db) =>
        {
            var movieExists = await db.Movies.AnyAsync(m => m.Id == movieId);
            if (!movieExists) return Results.NotFound();

            review.MovieId = movieId;
            review.CreatedAt = DateTime.UtcNow;

            db.Reviews.Add(review);
            await db.SaveChangesAsync();
            return Results.Created($"/api/movies/{movieId}/reviews/{review.Id}", review);
        });
    }
}
