using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Endpoints;

public static class MovieEndpoints
{
    public static void MapMovieEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/movies");

        group.MapGet("/", async (AppDbContext db, string? genre) =>
        {
            var query = db.Movies.AsQueryable();
            if (!string.IsNullOrEmpty(genre))
                query = query.Where(m => m.Genre == genre);

            return await query.OrderByDescending(m => m.CreatedAt).ToListAsync();
        });

        group.MapGet("/{id}", async (int id, AppDbContext db) =>
        {
            var movie = await db.Movies
                .Include(m => m.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            return movie is null ? Results.NotFound() : Results.Ok(movie);
        });

        group.MapPost("/", async (Movie movie, AppDbContext db) =>
        {
            movie.CreatedAt = DateTime.UtcNow;
            db.Movies.Add(movie);
            await db.SaveChangesAsync();
            return Results.Created($"/api/movies/{movie.Id}", movie);
        });

        group.MapPut("/{id}", async (int id, Movie input, AppDbContext db) =>
        {
            var movie = await db.Movies.FindAsync(id);
            if (movie is null) return Results.NotFound();

            movie.Title = input.Title;
            movie.Description = input.Description;
            movie.Genre = input.Genre;
            movie.ReleaseYear = input.ReleaseYear;
            movie.PosterUrl = input.PosterUrl;
            movie.Director = input.Director;

            await db.SaveChangesAsync();
            return Results.Ok(movie);
        });

        group.MapDelete("/{id}", async (int id, AppDbContext db) =>
        {
            var movie = await db.Movies.FindAsync(id);
            if (movie is null) return Results.NotFound();

            db.Movies.Remove(movie);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
