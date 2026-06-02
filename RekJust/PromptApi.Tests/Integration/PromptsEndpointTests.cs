using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using PromptApi.DTOs;
using PromptApi.Services;

namespace PromptApi.Tests.Integration;

/// <summary>
/// Testy integracyjne warstwy HTTP: routing, walidacja modelu, kody odpowiedzi, serializacja JSON.
/// Logika biznesowa jest mockowana przez TestWebApplicationFactory.
/// IClassFixture zapewnia jedną instancję fabryki dla wszystkich testów w klasie.
/// </summary>
public class PromptsEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public PromptsEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_ValidPrompt_Returns201Created()
    {
        var expected = new PromptResponseDto(
            Id: Guid.NewGuid(),
            Text: "Czym jest mikroserwis?",
            Status: "pending",
            Result: null,
            ErrorMessage: null,
            CreatedAt: DateTime.UtcNow,
            CompletedAt: null);

        _factory.PromptServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreatePromptDto>()))
            .ReturnsAsync(expected);

        var response = await _client.PostAsJsonAsync("/api/prompts",
            new { text = "Czym jest mikroserwis?" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull("201 powinien zawierać nagłówek Location");
    }

    [Fact]
    public async Task Post_ValidPrompt_ReturnsPromptDtoInBody()
    {
        var promptId = Guid.NewGuid();
        _factory.PromptServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreatePromptDto>()))
            .ReturnsAsync(new PromptResponseDto(promptId, "Test", "pending", null, null, DateTime.UtcNow, null));

        var response = await _client.PostAsJsonAsync("/api/prompts", new { text = "Test" });
        var body = await response.Content.ReadFromJsonAsync<PromptResponseDto>();

        body.Should().NotBeNull();
        body!.Id.Should().Be(promptId);
        body.Status.Should().Be("pending");
    }

    [Fact]
    public async Task Post_TextTooShort_Returns400BadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/prompts", new { text = "ab" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_EmptyText_Returns400BadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/prompts", new { text = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_MissingTextField_Returns400BadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/prompts", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_Returns200WithList()
    {
        var prompts = new List<PromptResponseDto>
        {
            new(Guid.NewGuid(), "Pierwszy", "completed", "Wynik", null, DateTime.UtcNow, DateTime.UtcNow),
            new(Guid.NewGuid(), "Drugi",    "pending",   null,    null, DateTime.UtcNow, null),
        };

        _factory.PromptServiceMock
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(prompts);

        var response = await _client.GetAsync("/api/prompts");
        var body = await response.Content.ReadFromJsonAsync<List<PromptResponseDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ExistingId_Returns200()
    {
        var id = Guid.NewGuid();
        _factory.PromptServiceMock
            .Setup(s => s.GetByIdAsync(id))
            .ReturnsAsync(new PromptResponseDto(id, "Test", "pending", null, null, DateTime.UtcNow, null));

        var response = await _client.GetAsync($"/api/prompts/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NonExistingId_Returns404()
    {
        _factory.PromptServiceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PromptResponseDto?)null);

        var response = await _client.GetAsync($"/api/prompts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Health_Returns200()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
