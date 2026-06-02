using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using PromptApi.Data;
using PromptApi.DTOs;
using PromptApi.Services;
using Testcontainers.PostgreSql;

namespace PromptApi.Tests.Integration;

/// <summary>
/// Testy integracyjne PromptService z prawdziwym PostgreSQL uruchamianym przez Testcontainers.
/// Weryfikują poprawność migracji EF Core, CHECK constraintów i indeksów na rzeczywistej bazie.
/// IAsyncLifetime: xUnit wywołuje InitializeAsync/DisposeAsync dla całej klasy (jeden kontener na zestaw testów).
/// </summary>
public class PromptServiceContainerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("rekjust_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();
    public async Task DisposeAsync() => await _postgres.StopAsync();

    private AppDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        var db = new AppDbContext(opts);
        db.Database.Migrate();
        return db;
    }

    [Fact]
    public async Task CreateAsync_WithRealDatabase_PersistsToPostgres()
    {
        using var db = CreateDb();
        var svc = new PromptService(db, new Mock<IPublishEndpoint>().Object);

        var result = await svc.CreateAsync(new CreatePromptDto { Text = "Test z PostgreSQL" });

        using var freshDb = CreateDb();
        var fromDb = await freshDb.Prompts.FindAsync(result.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Text.Should().Be("Test z PostgreSQL");
        fromDb.Status.Should().Be("pending");
    }

    [Fact]
    public async Task CreateAsync_CheckConstraint_RejectsInvalidStatus()
    {
        using var db = CreateDb();

        db.Prompts.Add(new PromptApi.Models.Prompt
        {
            Id = Guid.NewGuid(),
            Text = "Test",
            Status = "invalid_status",
            CreatedAt = DateTime.UtcNow
        });

        var act = () => db.SaveChangesAsync();
        await act.Should().ThrowAsync<Exception>("CHECK constraint zabrania wartości spoza zbioru");
    }

    [Fact]
    public async Task GetAllAsync_MultiplePrompts_UsesIndexForSorting()
    {
        using var db = CreateDb();
        var svc = new PromptService(db, new Mock<IPublishEndpoint>().Object);

        for (int i = 1; i <= 5; i++)
        {
            await svc.CreateAsync(new CreatePromptDto { Text = $"Prompt {i}" });
            await Task.Delay(5);
        }

        var results = (await svc.GetAllAsync()).ToList();

        results.Should().HaveCount(5);
        results.First().Text.Should().Be("Prompt 5");
        results.Last().Text.Should().Be("Prompt 1");
    }

    [Fact]
    public async Task Indexes_Exist_InDatabase()
    {
        using var db = CreateDb();

        var indexQuery = """
            SELECT indexname FROM pg_indexes
            WHERE tablename = 'Prompts'
            ORDER BY indexname
            """;

        var indexes = await db.Database
            .SqlQueryRaw<string>(indexQuery)
            .ToListAsync();

        indexes.Should().Contain("idx_prompts_status",      "indeks na Status przyspiesza filtrowanie Workera");
        indexes.Should().Contain("idx_prompts_created_at",  "indeks na CreatedAt przyspiesza sortowanie frontendu");
    }
}
