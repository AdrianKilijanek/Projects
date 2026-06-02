using Contracts;
using FluentAssertions;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using WorkerService.Consumers;
using WorkerService.Data;
using WorkerService.Models;

namespace WorkerService.Tests.Unit;

/// <summary>
/// Testy jednostkowe PromptCreatedConsumer. LLM zastąpiony mockiem IChatCompletionService.
/// Baza danych: SQLite InMemory (EF InMemory nie obsługuje ExecuteUpdateAsync wymaganego przez consumer).
/// </summary>
public class PromptCreatedConsumerTests
{
    // SQLite w trybie in-memory z Cache=Shared: baza istnieje dopóki jest otwarte połączenie.
    // Unikalny dbName na każdy test zapewnia izolację między testami.
    private static AppDbContext CreateDb()
    {
        var dbName = Guid.NewGuid().ToString("N");
        var conn = new SqliteConnection($"DataSource={dbName};Mode=Memory;Cache=Shared");
        conn.Open();
        var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options);
        db.Database.EnsureCreated();
        return db;
    }

    private static Kernel CreateKernel(IChatCompletionService chatService)
    {
        var services = new ServiceCollection();
        services.AddSingleton(chatService);
        return new Kernel(services.BuildServiceProvider());
    }

    private static ConsumeContext<PromptCreated> CreateContext(Guid promptId)
    {
        var ctx = new Mock<ConsumeContext<PromptCreated>>();
        ctx.Setup(c => c.Message).Returns(new PromptCreated(promptId));
        return ctx.Object;
    }

    // IChatCompletionService ma metodę GetChatMessageContentsAsync (plural).
    // GetChatMessageContentAsync (singular) to extension method wywołujący tę wyżej — mockujemy plural.
    private static Mock<IChatCompletionService> SetupChatMock(string response)
    {
        var mock = new Mock<IChatCompletionService>();
        mock.Setup(c => c.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(),
                It.IsAny<PromptExecutionSettings?>(),
                It.IsAny<Kernel?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessageContent>
            {
                new(AuthorRole.Assistant, response)
            });
        mock.Setup(c => c.Attributes).Returns(new Dictionary<string, object?>());
        return mock;
    }

    [Fact]
    public async Task Consume_PendingPrompt_ChangesStatusToCompleted()
    {
        using var db = CreateDb();
        var promptId = Guid.NewGuid();
        db.Prompts.Add(new Prompt
        {
            Id = promptId, Text = "Czym jest Docker?",
            Status = "pending", CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var chatMock = SetupChatMock("Docker to platforma konteneryzacji.");
        var consumer = new PromptCreatedConsumer(db, CreateKernel(chatMock.Object),
            NullLogger<PromptCreatedConsumer>.Instance);

        await consumer.Consume(CreateContext(promptId));

        // AsNoTracking omija cache EF change trackera — ExecuteUpdateAsync aktualizuje bazę z pominięciem trackera.
        var prompt = await db.Prompts.AsNoTracking().FirstAsync(p => p.Id == promptId);
        prompt.Status.Should().Be("completed");
        prompt.Result.Should().Be("Docker to platforma konteneryzacji.");
        prompt.CompletedAt.Should().NotBeNull();
        prompt.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Consume_PendingPrompt_CallsLlmWithCorrectText()
    {
        using var db = CreateDb();
        var promptId = Guid.NewGuid();
        db.Prompts.Add(new Prompt
        {
            Id = promptId, Text = "Wyjaśnij REST API",
            Status = "pending", CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var chatMock = SetupChatMock("REST API to...");
        var consumer = new PromptCreatedConsumer(db, CreateKernel(chatMock.Object),
            NullLogger<PromptCreatedConsumer>.Instance);

        await consumer.Consume(CreateContext(promptId));

        chatMock.Verify(c => c.GetChatMessageContentsAsync(
            It.Is<ChatHistory>(h => h.Any(m => m.Content!.Contains("Wyjaśnij REST API"))),
            It.IsAny<PromptExecutionSettings?>(),
            It.IsAny<Kernel?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Consume_AlreadyProcessingPrompt_SkipsWithoutCallingLlm()
    {
        // Weryfikuje idempotencję: at-least-once delivery może dostarczyć tę samą wiadomość dwa razy.
        // Consumer pomija prompt jeśli status != 'pending'.
        using var db = CreateDb();
        var promptId = Guid.NewGuid();
        db.Prompts.Add(new Prompt
        {
            Id = promptId, Text = "Test",
            Status = "processing",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var chatMock = new Mock<IChatCompletionService>();
        var consumer = new PromptCreatedConsumer(db, CreateKernel(chatMock.Object),
            NullLogger<PromptCreatedConsumer>.Instance);

        await consumer.Consume(CreateContext(promptId));

        chatMock.Verify(c => c.GetChatMessageContentsAsync(
            It.IsAny<ChatHistory>(), It.IsAny<PromptExecutionSettings?>(),
            It.IsAny<Kernel?>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Prompt w trakcie przetwarzania przez innego Workera nie powinien być przetworzony ponownie");
    }

    [Fact]
    public async Task Consume_AlreadyCompletedPrompt_SkipsWithoutCallingLlm()
    {
        using var db = CreateDb();
        var promptId = Guid.NewGuid();
        db.Prompts.Add(new Prompt
        {
            Id = promptId, Text = "Test",
            Status = "completed",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var chatMock = new Mock<IChatCompletionService>();
        var consumer = new PromptCreatedConsumer(db, CreateKernel(chatMock.Object),
            NullLogger<PromptCreatedConsumer>.Instance);

        await consumer.Consume(CreateContext(promptId));

        chatMock.Verify(c => c.GetChatMessageContentsAsync(
            It.IsAny<ChatHistory>(), It.IsAny<PromptExecutionSettings?>(),
            It.IsAny<Kernel?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Consume_LlmThrowsException_ChangesStatusToFailed()
    {
        // Weryfikuje że błąd LLM jest zapisywany jako status 'failed' i wyjątek jest re-rzucany dla MassTransit retry.
        using var db = CreateDb();
        var promptId = Guid.NewGuid();
        db.Prompts.Add(new Prompt
        {
            Id = promptId, Text = "Test",
            Status = "pending", CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var chatMock = new Mock<IChatCompletionService>();
        chatMock.Setup(c => c.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(), It.IsAny<PromptExecutionSettings?>(),
                It.IsAny<Kernel?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Ollama timeout after 30s"));
        chatMock.Setup(c => c.Attributes).Returns(new Dictionary<string, object?>());

        var consumer = new PromptCreatedConsumer(db, CreateKernel(chatMock.Object),
            NullLogger<PromptCreatedConsumer>.Instance);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => consumer.Consume(CreateContext(promptId)));

        var prompt = await db.Prompts.AsNoTracking().FirstAsync(p => p.Id == promptId);
        prompt.Status.Should().Be("failed");
        prompt.ErrorMessage.Should().Contain("Ollama timeout after 30s");
    }

    [Fact]
    public async Task Consume_NonExistentPromptId_DoesNotThrow()
    {
        using var db = CreateDb();
        var chatMock = new Mock<IChatCompletionService>();
        var consumer = new PromptCreatedConsumer(db, CreateKernel(chatMock.Object),
            NullLogger<PromptCreatedConsumer>.Instance);

        var act = () => consumer.Consume(CreateContext(Guid.NewGuid()));
        await act.Should().NotThrowAsync();
    }
}
