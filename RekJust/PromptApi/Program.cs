using System.ComponentModel.DataAnnotations;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PromptApi.Data;
using PromptApi.DTOs;
using PromptApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
    });
});

// Rejestracja przez interfejs umożliwia podmianę na mock w testach.
builder.Services.AddScoped<IPromptService, PromptService>();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddOpenApi();

var app = builder.Build();

// Migracje pomijamy w środowisku "Testing", żeby uniknąć konfliktu providerów EF Core w testach.
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.UseHttpsRedirection();

// POST /api/prompts — tworzy nowy prompt, zwraca 201 z nagłówkiem Location.
// Minimal API nie waliduje DataAnnotations automatycznie w przeciwieństwie do [ApiController].
app.MapPost("/api/prompts", async (CreatePromptDto dto, IPromptService svc) =>
{
    var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
    if (!Validator.TryValidateObject(dto, new ValidationContext(dto), errors, validateAllProperties: true))
    {
        return Results.ValidationProblem(errors
            .GroupBy(e => e.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage ?? "").ToArray()));
    }

    var prompt = await svc.CreateAsync(dto);
    return Results.Created($"/api/prompts/{prompt.Id}", prompt);
})
.WithName("CreatePrompt");

// GET /api/prompts — zwraca wszystkie prompty posortowane od najnowszego (używane przez polling frontendu).
app.MapGet("/api/prompts", async (IPromptService svc) =>
    Results.Ok(await svc.GetAllAsync()))
.WithName("GetPrompts");

// GET /api/prompts/{id} — zwraca pojedynczy prompt po ID lub 404 jeśli nie istnieje.
app.MapGet("/api/prompts/{id:guid}", async (Guid id, IPromptService svc) =>
{
    var prompt = await svc.GetByIdAsync(id);
    return prompt is null ? Results.NotFound() : Results.Ok(prompt);
})
.WithName("GetPromptById");

// GET /health — liveness probe dla Docker health check.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

// Wymagane przez WebApplicationFactory w testach integracyjnych.
public partial class Program { }
