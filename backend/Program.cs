using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Endpoints;
using Backend.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Movies.Any())
    {
        db.Movies.AddRange(
            new Movie { Title = "Sueño de fuga", Description = "Dos hombres encarcelados forjan una amistad a lo largo de los años.", Genre = "Drama", ReleaseYear = 1994, Director = "Frank Darabont" },
            new Movie { Title = "El Padrino", Description = "El envejecido patriarca de una dinastía del crimen organizado.", Genre = "Crimen", ReleaseYear = 1972, Director = "Francis Ford Coppola" },
            new Movie { Title = "Tiempos violentos", Description = "Las vidas de dos sicarios, un boxeador y más.", Genre = "Crimen", ReleaseYear = 1994, Director = "Quentin Tarantino" },
            new Movie { Title = "El origen", Description = "Un ladrón que roba secretos corporativos a través de los sueños.", Genre = "Ciencia ficción", ReleaseYear = 2010, Director = "Christopher Nolan" },
            new Movie { Title = "Interestelar", Description = "Un equipo de exploradores viaja a través de un agujero de gusano.", Genre = "Ciencia ficción", ReleaseYear = 2014, Director = "Christopher Nolan" },
            new Movie { Title = "Matrix", Description = "Un hacker descubre la verdadera naturaleza de la realidad.", Genre = "Ciencia ficción", ReleaseYear = 1999, Director = "Lana Wachowski" },
            new Movie { Title = "Parásitos", Description = "La avaricia y la discriminación amenazan una relación simbiótica.", Genre = "Drama", ReleaseYear = 2019, Director = "Bong Joon-ho" },
            new Movie { Title = "El viaje de Chihiro", Description = "Una niña trabaja en una casa de baños del mundo espiritual.", Genre = "Animación", ReleaseYear = 2001, Director = "Hayao Miyazaki" },
            new Movie { Title = "Yu-Gi-Oh! El lado oscuro de las dimensiones", Description = "Yugi y sus amigos enfrentan una nueva amenaza dimensional.", Genre = "Animación", ReleaseYear = 2016, Director = "Satoshi Kuwabara" }
        );
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapMovieEndpoints();
app.MapReviewEndpoints();
app.MapWatchlistEndpoints();

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
