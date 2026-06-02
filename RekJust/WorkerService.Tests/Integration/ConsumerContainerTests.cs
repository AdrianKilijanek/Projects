using Contracts;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Testcontainers.PostgreSql;
using WorkerService.Consumers;
using WorkerService.Data;
using WorkerService.Models;

namespace WorkerService.Tests.Integration;

/// <summary>
/// Testy integracyjne PromptCreatedConsumer z prawdziwym PostgreSQL uruchamianym przez Testcontainers.
/// Weryfikują atomowy UPDATE z warunkiem AND status='pending' na rzeczywistej bazie.
/// </summary>
public class ConsumerContainerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("worker_test")
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
        // WorkerService nie posiada migracji — schemat tworzony bezpośrednio z modelu.
        db.Database.EnsureCreated();
        return db;
    }

    private static Kernel CreateKernelWithResponse(string response)
    {
        var chatMock = new Mock<IChatCompletionService>();
        chatMock.Setup(c => c.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(), It.IsAny<PromptExecutionSettings?>(),
                It.IsAny<Kernel?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessageContent> { new(AuthorRole.Assistant, response) });
        chatMock.Setup(c => c.Attributes).Returns(new Dictionary<string, object?>());

        var services = new ServiceCollection();
        services.AddSingleton(chatMock.Object);
        return new Kernel(services.BuildServiceProvider());
    }

    private static ConsumeContext<PromptCreated> CreateContext(Guid promptId)
    {
        var ctx = new Mock<ConsumeContext<PromptCreated>>();
        ctx.Setup(c => c.Message).Returns(new PromptCreated(promptId));
        return ctx.Object;
    }

    [Fact]
    public async Task Consume_WithRealPostgres_UpdatesStatusCorrectly()
    {
        using var db = CreateDb();
        var promptId = Guid.NewGuid();
        db.Prompts.Add(new Prompt
        {
            Id = promptId,
            Text = "Test integracyjny z prawdziwą bazą",
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var consumer = new PromptCreatedConsumer(db, CreateKernelWithResponse("Odpowiedź LLM"),
            NullLogger<PromptCreatedConsumer>.Instance);
        await consumer.Consume(CreateContext(promptId));

        // AsNoTracking omija EF change tracker (ExecuteUpdateAsync go nie aktualizuje).
        var result = await db.Prompts.AsNoTracking().FirstAsync(p => p.Id == promptId);
        result!.Status.Should().Be("completed");
        result.Result.Should().Be("Odpowiedź LLM");
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Consume_IdempotencyWithRealDatabase_SecondCallIsNoop()
    {
        // Symuluje at-least-once delivery: ta sama wiadomość dostarczona dwa razy.
        // Atomowy UPDATE WHERE status='pending' gwarantuje że LLM zostanie wywołany tylko raz.
        using var db = CreateDb();
        var promptId = Guid.NewGuid();
        db.Prompts.Add(new Prompt
        {
            Id = promptId, Text = "Test idempotencji",
            Status = "pending", CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var callCount = 0;
        var chatMock = new Mock<IChatCompletionService>();
        chatMock.Setup(c => c.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(), It.IsAny<PromptExecutionSettings?>(),
                It.IsAny<Kernel?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return new List<ChatMessageContent> { new(AuthorRole.Assistant, "Wynik") };
            });
        chatMock.Setup(c => c.Attributes).Returns(new Dictionary<string, object?>());

        var services = new ServiceCollection();
        services.AddSingleton(chatMock.Object);
        var kernel = new Kernel(services.BuildServiceProvider());
        var consumer = new PromptCreatedConsumer(db, kernel,
            NullLogger<PromptCreatedConsumer>.Instance);

        await consumer.Consume(CreateContext(promptId));
        await consumer.Consume(CreateContext(promptId));

        callCount.Should().Be(1, "drugi call powinien zostać odrzucony przez atomowy UPDATE z warunkiem");

        var prompt = await db.Prompts.AsNoTracking().FirstAsync(p => p.Id == promptId);
        prompt!.Status.Should().Be("completed");
    }
}
