using Contracts;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using PromptApi.Data;
using PromptApi.DTOs;
using PromptApi.Services;

namespace PromptApi.Tests.Unit;

/// <summary>
/// Testy jednostkowe PromptService. Baza danych: EF Core InMemory. Kolejka: Mock IPublishEndpoint.
/// </summary>
public class PromptServiceTests
{
    private static AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task CreateAsync_ValidText_ReturnsDtoWithPendingStatus()
    {
        using var db = CreateDb();
        var svc = new PromptService(db, new Mock<IPublishEndpoint>().Object);

        var result = await svc.CreateAsync(new CreatePromptDto { Text = "Czym jest Docker?" });

        result.Status.Should().Be("pending");
        result.Text.Should().Be("Czym jest Docker?");
        result.Id.Should().NotBeEmpty();
        result.Result.Should().BeNull();
        result.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidText_SavesPromptToDatabase()
    {
        using var db = CreateDb();
        var svc = new PromptService(db, new Mock<IPublishEndpoint>().Object);

        var result = await svc.CreateAsync(new CreatePromptDto { Text = "Testowy prompt" });

        var saved = await db.Prompts.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Text.Should().Be("Testowy prompt");
        saved.Status.Should().Be("pending");
    }

    [Fact]
    public async Task CreateAsync_ValidText_PublishesPromptCreatedEvent()
    {
        using var db = CreateDb();
        var publishMock = new Mock<IPublishEndpoint>();
        var svc = new PromptService(db, publishMock.Object);

        var result = await svc.CreateAsync(new CreatePromptDto { Text = "Test" });

        publishMock.Verify(
            p => p.Publish(It.Is<PromptCreated>(m => m.PromptId == result.Id), default),
            Times.Once,
            "Po zapisie do DB powinno być opublikowane zdarzenie PromptCreated");
    }

    [Fact]
    public async Task GetAllAsync_MultiplePrompts_ReturnsNewestFirst()
    {
        using var db = CreateDb();
        var svc = new PromptService(db, new Mock<IPublishEndpoint>().Object);

        await svc.CreateAsync(new CreatePromptDto { Text = "Pierwszy" });
        await Task.Delay(5);
        await svc.CreateAsync(new CreatePromptDto { Text = "Drugi" });

        var all = (await svc.GetAllAsync()).ToList();

        all.Should().HaveCount(2);
        all[0].Text.Should().Be("Drugi");
        all[1].Text.Should().Be("Pierwszy");
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyCollection()
    {
        using var db = CreateDb();
        var svc = new PromptService(db, new Mock<IPublishEndpoint>().Object);

        var result = await svc.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectPrompt()
    {
        using var db = CreateDb();
        var svc = new PromptService(db, new Mock<IPublishEndpoint>().Object);
        var created = await svc.CreateAsync(new CreatePromptDto { Text = "Szukany" });

        var result = await svc.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Text.Should().Be("Szukany");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        using var db = CreateDb();
        var svc = new PromptService(db, new Mock<IPublishEndpoint>().Object);

        var result = await svc.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
}
