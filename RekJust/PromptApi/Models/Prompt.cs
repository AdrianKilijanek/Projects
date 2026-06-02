namespace PromptApi.Models;

/// <summary>
/// Encja reprezentująca prompt użytkownika. Mapuje się na tabelę Prompts w PostgreSQL.
/// </summary>
public class Prompt
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Status { get; set; } = "pending"; // pending | processing | completed | failed
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
