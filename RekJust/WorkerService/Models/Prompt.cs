namespace WorkerService.Models;

// Worker ma własną kopię encji – każdy mikroserwis zarządza swoim modelem.
// Nie współdzielimy encji bazy przez bibliotekę Contracts.
// Contracts zawiera TYLKO typy wiadomości kolejkowych.
public class Prompt
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
