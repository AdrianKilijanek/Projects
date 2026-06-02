namespace Contracts;

/// <summary>
/// Zdarzenie publikowane przez PromptApi po zapisaniu promptu do bazy.
/// Typ musi być identyczny w PromptApi i WorkerService — stąd osobna biblioteka współdzielona.
/// </summary>
public record PromptCreated(Guid PromptId);
