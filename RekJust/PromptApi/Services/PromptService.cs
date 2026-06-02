using MassTransit;
using Microsoft.EntityFrameworkCore;
using PromptApi.Data;
using PromptApi.DTOs;
using PromptApi.Models;
using Contracts;

namespace PromptApi.Services;

/// <summary>
/// Zarządza cyklem życia promptów: zapis do bazy i publikacja zdarzenia do kolejki.
/// </summary>
public interface IPromptService
{
    Task<PromptResponseDto> CreateAsync(CreatePromptDto dto);
    Task<IEnumerable<PromptResponseDto>> GetAllAsync();
    Task<PromptResponseDto?> GetByIdAsync(Guid id);
}

public class PromptService(AppDbContext db, IPublishEndpoint publishEndpoint) : IPromptService
{
    public async Task<PromptResponseDto> CreateAsync(CreatePromptDto dto)
    {
        var prompt = new Prompt { Text = dto.Text };
        db.Prompts.Add(prompt);
        await db.SaveChangesAsync();

        // Publikacja po zapisie do bazy (dual-write). Outbox Pattern nie jest tu zaimplementowany.
        await publishEndpoint.Publish(new PromptCreated(prompt.Id));

        return ToDto(prompt);
    }

    public async Task<IEnumerable<PromptResponseDto>> GetAllAsync()
    {
        return await db.Prompts
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<PromptResponseDto?> GetByIdAsync(Guid id)
    {
        var prompt = await db.Prompts.FindAsync(id);
        return prompt is null ? null : ToDto(prompt);
    }

    private static PromptResponseDto ToDto(Prompt p) => new(
        p.Id, p.Text, p.Status, p.Result, p.ErrorMessage, p.CreatedAt, p.CompletedAt);
}
